using Polly.Telemetry;

namespace Polly.CircuitBreaker;

/// <summary>
/// Thread-safe controller that holds and manages the circuit breaker state transitions.
/// </summary>
internal sealed class CircuitStateController<T> : IDisposable
{
    private readonly object _lock = new();
    private readonly ScheduledTaskExecutor _executor = new();
    private readonly Func<OnCircuitOpenedArguments<T>, ValueTask>? _onOpened;
    private readonly Func<OnCircuitClosedArguments<T>, ValueTask>? _onClosed;
    private readonly Func<OnCircuitHalfOpenedArguments, ValueTask>? _onHalfOpen;
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly CircuitBehavior _behavior;
    private readonly TimeSpan _breakDuration;
    private readonly Func<BreakDurationGeneratorArguments, ValueTask<TimeSpan>>? _breakDurationGenerator;
    private DateTimeOffset _blockedUntil;
    private CircuitState _circuitState = CircuitState.Closed;
    private Outcome<T>? _lastOutcome;
    private Exception? _breakingException;
    private bool _disposed;
    private int _halfOpenAttempts;

#pragma warning disable S107
    public CircuitStateController(
        TimeSpan breakDuration,
        Func<OnCircuitOpenedArguments<T>, ValueTask>? onOpened,
        Func<OnCircuitClosedArguments<T>, ValueTask>? onClosed,
        Func<OnCircuitHalfOpenedArguments, ValueTask>? onHalfOpen,
        CircuitBehavior behavior,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry,
        Func<BreakDurationGeneratorArguments, ValueTask<TimeSpan>>? breakDurationGenerator)
#pragma warning restore S107
    {
        _breakDuration = breakDuration;
        _onOpened = onOpened;
        _onClosed = onClosed;
        _onHalfOpen = onHalfOpen;
        _behavior = behavior;
        _timeProvider = timeProvider;
        _telemetry = telemetry;
        _breakDurationGenerator = breakDurationGenerator;
    }

    public CircuitState CircuitState
    {
        get
        {
            EnsureNotDisposed();

            lock (_lock)
            {
                return _circuitState;
            }
        }
    }

    public Exception? LastException
    {
        get
        {
            EnsureNotDisposed();

            lock (_lock)
            {
                return _lastOutcome?.Exception;
            }
        }
    }

    public Outcome<T>? LastHandledOutcome
    {
        get
        {
            EnsureNotDisposed();

            lock (_lock)
            {
                return _lastOutcome;
            }
        }
    }

    public Task IsolateCircuitAsync(ResilienceContext context)
    {
        EnsureNotDisposed();

        context.Initialize<T>(isSynchronous: false);

        Task task;

        lock (_lock)
        {
            var exception = new IsolatedCircuitException();
            _telemetry.SetTelemetrySource(exception);
            SetLastHandledOutcome_NeedsLock(Outcome.FromException<T>(exception));
            task = OpenCircuitFor_NeedsLock(Outcome.FromResult<T>(default), TimeSpan.MaxValue, manual: true, context);
            _circuitState = CircuitState.Isolated;
        }

        return ExecuteScheduledTaskAsync(task, context);
    }

    public Task CloseCircuitAsync(ResilienceContext context)
    {
        EnsureNotDisposed();

        context.Initialize<T>(isSynchronous: false);

        Task task;

        lock (_lock)
        {
            task = CloseCircuit_NeedsLock(Outcome.FromResult<T>(default), manual: true, context);
        }

        return ExecuteScheduledTaskAsync(task, context);
    }

    public ValueTask<Outcome<T>?> OnActionPreExecuteAsync(ResilienceContext context)
    {
        EnsureNotDisposed();

        BrokenCircuitException? exception = null;
        bool isHalfOpen = false;

        var task = Task.CompletedTask;

        lock (_lock)
        {
            // check if circuit can be half-opened
            if (_circuitState == CircuitState.Open && PermitHalfOpenCircuitTest_NeedsLock())
            {
                _halfOpenAttempts++;
                _circuitState = CircuitState.HalfOpen;
                _telemetry.Report(new(ResilienceEventSeverity.Warning, CircuitBreakerConstants.OnHalfOpenEvent), context, new OnCircuitHalfOpenedArguments(context));
                isHalfOpen = true;
            }

            exception = _circuitState switch
            {
                CircuitState.Open => CreateBrokenCircuitException(),
                CircuitState.HalfOpen when !isHalfOpen => CreateBrokenCircuitException(),
                CircuitState.Isolated => new IsolatedCircuitException(),
                _ => null
            };

            if (isHalfOpen && _onHalfOpen is not null)
            {
                task = ScheduleHalfOpenTask(context);
            }
        }

        if (exception is not null)
        {
            _telemetry.SetTelemetrySource(exception);
            return new(result: new(exception));
        }

        task = ExecuteScheduledTaskAsync(task, context);
        if (!task.IsCompleted)
        {
            return WaitHalfOpenTask(task, context.ContinueOnCapturedContext);
        }

#pragma warning disable CA1849 // Call async methods when in an async method
        Debug.Assert(task.IsCompleted, "Async flow is handled separately");
        task.GetAwaiter().GetResult();
#pragma warning restore CA1849
        return default;
    }

    private static async ValueTask<Outcome<T>?> WaitHalfOpenTask(Task task, bool continueOnCapturedContext)
    {
        await task.ConfigureAwait(continueOnCapturedContext);
        return null;
    }

    public Task OnUnhandledOutcomeAsync(Outcome<T> outcome, ResilienceContext context)
    {
        EnsureNotDisposed();

        var task = Task.CompletedTask;

        lock (_lock)
        {
            _behavior.OnActionSuccess(_circuitState);

            // Circuit state handling:
            //
            // HalfOpen - close the circuit
            // Closed - do nothing
            // Open, Isolated -  A successful call result may arrive when the circuit is open, if it was placed before the circuit broke.
            // We take no special action; only time passing governs transitioning from Open to HalfOpen state.
            if (_circuitState == CircuitState.HalfOpen)
            {
                task = CloseCircuit_NeedsLock(outcome, manual: false, context);
            }

        }

        return ExecuteScheduledTaskAsync(task, context);
    }

    public Task OnHandledOutcomeAsync(Outcome<T> outcome, ResilienceContext context)
    {
        EnsureNotDisposed();

        var task = Task.CompletedTask;

        lock (_lock)
        {
            SetLastHandledOutcome_NeedsLock(outcome);

            _behavior.OnActionFailure(_circuitState, out var shouldBreak);

            // Circuit state handling
            // HalfOpen - open the circuit again
            // Closed - break the circuit if the behavior indicates it
            // Open, Isolated - a failure call result may arrive when the circuit is open,
            // if it was placed before the circuit broke. We take no action beyond tracking
            // the metric; we do not want to duplicate-signal onBreak; we do not want to extend time for which the circuit is broken.
            // We do not want to mask the fact that the call executed (as replacing its result with a Broken/IsolatedCircuitException would do).

            if (_circuitState == CircuitState.HalfOpen || (_circuitState == CircuitState.Closed && shouldBreak))
            {
                task = OpenCircuitFor_NeedsLock(outcome, _breakDuration, manual: false, context);
            }
        }

        return ExecuteScheduledTaskAsync(task, context);
    }

    public void Dispose()
    {
        _executor.Dispose();
        _disposed = true;
    }

    internal static Task ExecuteScheduledTaskAsync(Task task, ResilienceContext context)
    {
        if (context.IsSynchronous && !task.IsCompleted)
        {
#pragma warning disable CA1849 // Call async methods when in an async method
            // because this is synchronous execution we need to block
            task.GetAwaiter().GetResult();
#pragma warning restore CA1849 // Call async methods when in an async method
        }

        return task;
    }

    private static bool IsDateTimeOverflow(DateTimeOffset utcNow, TimeSpan breakDuration)
    {
        TimeSpan maxDifference = DateTimeOffset.MaxValue - utcNow;

        // stryker disable once equality : no means to test this
        return breakDuration > maxDifference;
    }

#if NET8_0_OR_GREATER
    private void EnsureNotDisposed()
        => ObjectDisposedException.ThrowIf(_disposed, this);
#else
    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(CircuitStateController<T>));
        }
    }
#endif

    private Task CloseCircuit_NeedsLock(Outcome<T> outcome, bool manual, ResilienceContext context)
    {
        _blockedUntil = DateTimeOffset.MinValue;
        _lastOutcome = null;
        _halfOpenAttempts = 0;

        CircuitState priorState = _circuitState;
        _circuitState = CircuitState.Closed;
        _behavior.OnCircuitClosed();

        if (priorState != CircuitState.Closed)
        {
            var args = new OnCircuitClosedArguments<T>(context, outcome, manual);
            _telemetry.Report<OnCircuitClosedArguments<T>, T>(new(ResilienceEventSeverity.Information, CircuitBreakerConstants.OnCircuitClosed), args);

            if (_onClosed is not null)
            {
                return _executor.ScheduleTask(() => _onClosed(args).AsTask());
            }
        }

        return Task.CompletedTask;
    }

    private bool PermitHalfOpenCircuitTest_NeedsLock()
    {
        var now = _timeProvider.GetUtcNow();
        if (now >= _blockedUntil)
        {
            _blockedUntil = now + _breakDuration;
            return true;
        }

        return false;
    }

    private void SetLastHandledOutcome_NeedsLock(Outcome<T> outcome)
    {
        _lastOutcome = outcome;
        _breakingException = outcome.Exception;
    }

    private BrokenCircuitException CreateBrokenCircuitException()
    {
        TimeSpan retryAfter = _blockedUntil - _timeProvider.GetUtcNow();
        var exception = new BrokenCircuitException(BrokenCircuitException.DefaultMessage, retryAfter, _breakingException!);
        _telemetry.SetTelemetrySource(exception);
        return exception;
    }

    private Task OpenCircuitFor_NeedsLock(Outcome<T> outcome, TimeSpan breakDuration, bool manual, ResilienceContext context)
    {
        var utcNow = _timeProvider.GetUtcNow();

        if (_breakDurationGenerator is not null)
        {
#pragma warning disable CA2012
#pragma warning disable S1226
            breakDuration = _breakDurationGenerator(new(_behavior.FailureRate, _behavior.FailureCount, context, _halfOpenAttempts)).GetAwaiter().GetResult();
#pragma warning restore S1226
#pragma warning restore CA2012
        }

        _blockedUntil = IsDateTimeOverflow(utcNow, breakDuration) ? DateTimeOffset.MaxValue : utcNow + breakDuration;
        _circuitState = CircuitState.Open;

        var args = new OnCircuitOpenedArguments<T>(context, outcome, breakDuration, manual);
        _telemetry.Report<OnCircuitOpenedArguments<T>, T>(new(ResilienceEventSeverity.Error, CircuitBreakerConstants.OnCircuitOpened), args);

        if (_onOpened is not null)
        {
            return _executor.ScheduleTask(() => _onOpened(args).AsTask());
        }

        return Task.CompletedTask;
    }

    private Task ScheduleHalfOpenTask(ResilienceContext context)
    {
        return _executor.ScheduleTask(() => _onHalfOpen!(new OnCircuitHalfOpenedArguments(context)).AsTask());
    }
}

