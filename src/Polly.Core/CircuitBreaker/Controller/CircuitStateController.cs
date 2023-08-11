using Polly.Telemetry;

namespace Polly.CircuitBreaker;

/// <summary>
/// Thread-safe controller that holds and manages the circuit breaker state transitions.
/// </summary>
internal sealed class CircuitStateController<T> : IDisposable
{
    private readonly object _lock = new();
    private readonly ScheduledTaskExecutor _executor = new();
    private readonly Func<OutcomeArguments<T, OnCircuitOpenedArguments>, ValueTask>? _onOpened;
    private readonly Func<OutcomeArguments<T, OnCircuitClosedArguments>, ValueTask>? _onClosed;
    private readonly Func<OnCircuitHalfOpenedArguments, ValueTask>? _onHalfOpen;
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly CircuitBehavior _behavior;
    private readonly TimeSpan _breakDuration;
    private DateTimeOffset _blockedUntil;
    private CircuitState _circuitState = CircuitState.Closed;
    private Outcome<object>? _lastOutcome;
    private BrokenCircuitException _breakingException = new();
    private bool _disposed;

    public CircuitStateController(
        TimeSpan breakDuration,
        Func<OutcomeArguments<T, OnCircuitOpenedArguments>, ValueTask>? onOpened,
        Func<OutcomeArguments<T, OnCircuitClosedArguments>, ValueTask>? onClosed,
        Func<OnCircuitHalfOpenedArguments, ValueTask>? onHalfOpen,
        CircuitBehavior behavior,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry)
    {
        _breakDuration = breakDuration;
        _onOpened = onOpened;
        _onClosed = onClosed;
        _onHalfOpen = onHalfOpen;
        _behavior = behavior;
        _timeProvider = timeProvider;
        _telemetry = telemetry;
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

    public Outcome<object>? LastHandledOutcome
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

    public ValueTask IsolateCircuitAsync(ResilienceContext context)
    {
        EnsureNotDisposed();

        context.Initialize<T>(isSynchronous: false);

        Task? task;

        lock (_lock)
        {
            SetLastHandledOutcome_NeedsLock(Outcome.FromException<T>(new IsolatedCircuitException()));
            OpenCircuitFor_NeedsLock(Outcome.FromResult<T>(default), TimeSpan.MaxValue, manual: true, context, out task);
            _circuitState = CircuitState.Isolated;
        }

        return ExecuteScheduledTaskAsync(task, context);
    }

    public ValueTask CloseCircuitAsync(ResilienceContext context)
    {
        EnsureNotDisposed();

        context.Initialize<T>(isSynchronous: false);

        Task? task;

        lock (_lock)
        {
            CloseCircuit_NeedsLock(Outcome.FromResult<T>(default), manual: true, context, out task);
        }

        return ExecuteScheduledTaskAsync(task, context);
    }

    public async ValueTask<Outcome<T>?> OnActionPreExecuteAsync(ResilienceContext context)
    {
        EnsureNotDisposed();

        Exception? exception = null;
        bool isHalfOpen = false;

        Task? task = null;

        lock (_lock)
        {
            // check if circuit can be half-opened
            if (_circuitState == CircuitState.Open && PermitHalfOpenCircuitTest_NeedsLock())
            {
                _circuitState = CircuitState.HalfOpen;
                _telemetry.Report(new(ResilienceEventSeverity.Warning, CircuitBreakerConstants.OnHalfOpenEvent), context, new OnCircuitHalfOpenedArguments(context));
                isHalfOpen = true;
            }

            exception = _circuitState switch
            {
                CircuitState.Open => _breakingException,
                CircuitState.HalfOpen when !isHalfOpen => _breakingException,
                CircuitState.Isolated => new IsolatedCircuitException(),
                _ => null
            };

            if (isHalfOpen && _onHalfOpen is not null)
            {
                task = ScheduleHalfOpenTask(context);
            }
        }

        await ExecuteScheduledTaskAsync(task, context).ConfigureAwait(context.ContinueOnCapturedContext);

        if (exception is not null)
        {
            return Outcome.FromException<T>(exception);
        }

        return null;
    }

    public ValueTask OnActionSuccessAsync(Outcome<T> outcome, ResilienceContext context)
    {
        EnsureNotDisposed();

        Task? task = null;

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
                CloseCircuit_NeedsLock(outcome, manual: false, context, out task);
            }

        }

        return ExecuteScheduledTaskAsync(task, context);
    }

    public ValueTask OnActionFailureAsync(Outcome<T> outcome, ResilienceContext context)
    {
        EnsureNotDisposed();

        Task? task = null;

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

            if (_circuitState == CircuitState.HalfOpen)
            {
                OpenCircuit_NeedsLock(outcome, manual: false, context, out task);
            }
            else if (_circuitState == CircuitState.Closed && shouldBreak)
            {
                OpenCircuit_NeedsLock(outcome, manual: false, context, out task);
            }
        }

        return ExecuteScheduledTaskAsync(task, context);
    }

    public void Dispose()
    {
        _executor.Dispose();
        _disposed = true;
    }

    internal static async ValueTask ExecuteScheduledTaskAsync(Task? task, ResilienceContext context)
    {
        if (task is not null)
        {
            if (context.IsSynchronous)
            {
#pragma warning disable CA1849 // Call async methods when in an async method
                // because this is synchronous execution we need to block
                task.GetAwaiter().GetResult();
#pragma warning restore CA1849 // Call async methods when in an async method
            }
            else
            {
                await task.ConfigureAwait(context.ContinueOnCapturedContext);
            }
        }
    }

    private static bool IsDateTimeOverflow(DateTimeOffset utcNow, TimeSpan breakDuration)
    {
        TimeSpan maxDifference = DateTime.MaxValue - utcNow;

        // stryker disable once equality : no means to test this
        return breakDuration > maxDifference;
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(CircuitStateController<T>));
        }
    }

    private void CloseCircuit_NeedsLock(Outcome<T> outcome, bool manual, ResilienceContext context, out Task? scheduledTask)
    {
        scheduledTask = null;

        _blockedUntil = DateTimeOffset.MinValue;
        _lastOutcome = null;

        CircuitState priorState = _circuitState;
        _circuitState = CircuitState.Closed;
        _behavior.OnCircuitClosed();

        if (priorState != CircuitState.Closed)
        {
            var args = new OutcomeArguments<T, OnCircuitClosedArguments>(context, outcome, new OnCircuitClosedArguments(manual));
            _telemetry.Report(new(ResilienceEventSeverity.Information, CircuitBreakerConstants.OnCircuitClosed), args);

            if (_onClosed is not null)
            {
                _executor.ScheduleTask(() => _onClosed(args).AsTask(), context, out scheduledTask);
            }
        }
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

    private void SetLastHandledOutcome_NeedsLock<TResult>(Outcome<TResult> outcome)
    {
        _lastOutcome = outcome.AsOutcome();

        if (outcome.Exception is Exception exception)
        {
            _breakingException = new BrokenCircuitException(BrokenCircuitException.DefaultMessage, exception);
        }
        else if (outcome.TryGetResult(out var result))
        {
            _breakingException = new BrokenCircuitException<TResult>(BrokenCircuitException.DefaultMessage, result!);
        }
    }

    private void OpenCircuit_NeedsLock(Outcome<T> outcome, bool manual, ResilienceContext context, out Task? scheduledTask)
    {
        OpenCircuitFor_NeedsLock(outcome, _breakDuration, manual, context, out scheduledTask);
    }

    private void OpenCircuitFor_NeedsLock(Outcome<T> outcome, TimeSpan breakDuration, bool manual, ResilienceContext context, out Task? scheduledTask)
    {
        scheduledTask = null;
        var utcNow = _timeProvider.GetUtcNow();

        _blockedUntil = IsDateTimeOverflow(utcNow, breakDuration) ? DateTimeOffset.MaxValue : utcNow + breakDuration;

        var transitionedState = _circuitState;
        _circuitState = CircuitState.Open;

        var args = new OutcomeArguments<T, OnCircuitOpenedArguments>(context, outcome, new OnCircuitOpenedArguments(breakDuration, manual));
        _telemetry.Report(new(ResilienceEventSeverity.Error, CircuitBreakerConstants.OnCircuitOpened), args);

        if (_onOpened is not null)
        {
            _executor.ScheduleTask(() => _onOpened(args).AsTask(), context, out scheduledTask);
        }
    }

    private Task ScheduleHalfOpenTask(ResilienceContext context)
    {
        _executor.ScheduleTask(() => _onHalfOpen!(new OnCircuitHalfOpenedArguments(context)).AsTask(), context, out var task);
        return task;
    }
}

