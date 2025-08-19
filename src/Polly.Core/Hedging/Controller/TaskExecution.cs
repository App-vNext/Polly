using Polly.Hedging.Utils;
using Polly.Telemetry;

namespace Polly.Hedging.Controller;

#pragma warning disable CA1031 // Do not catch general exception types

/// <summary>
/// Represents a single hedging attempt execution alongside all the necessary resources. These are:
///
/// <list type="bullet">
/// <item>
/// Distinct <see cref="ResilienceContext"/> instance for this execution.
/// One exception are primary task where the main context is reused.
/// </item>
/// <item>
/// The cancellation token associated with the execution.
/// </item>
/// </list>
/// </summary>
internal sealed class TaskExecution<T>
{
    private readonly ResilienceContext _cachedContext = ResilienceContextPool.Shared.Get();
    private readonly CancellationTokenSourcePool _cancellationTokenSourcePool;
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly HedgingHandler<T> _handler;
    private CancellationTokenSource? _cancellationSource;
    private CancellationTokenRegistration _cancellationRegistration;
    private ResilienceContext? _activeContext;
    private long _startExecutionTimestamp;
    private long _stopExecutionTimestamp;

    public TaskExecution(HedgingHandler<T> handler, CancellationTokenSourcePool cancellationTokenSourcePool, TimeProvider timeProvider, ResilienceStrategyTelemetry telemetry)
    {
        _handler = handler;
        _cancellationTokenSourcePool = cancellationTokenSourcePool;
        _timeProvider = timeProvider;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Gets the task that represents the execution of the hedged task.
    /// </summary>
    /// <remarks>
    /// This property is not-null once the <see cref="TaskExecution{T}"/> is initialized.
    /// Awaiting this task will never throw as all exceptions are caught and stored
    /// into <see cref="Outcome"/> property.
    /// </remarks>
    public Task? ExecutionTaskSafe { get; private set; }

    public Outcome<T> Outcome { get; private set; }

    public bool IsHandled { get; private set; }

    public bool IsAccepted { get; private set; }

    public ResilienceContext Context => _activeContext ?? throw new InvalidOperationException("TaskExecution is not initialized.");

    public HedgedTaskType Type { get; set; }

    public Action<TaskExecution<T>>? OnReset { get; set; }

    public TimeSpan ExecutionTime => _timeProvider.GetElapsedTime(_startExecutionTimestamp, _stopExecutionTimestamp);

    public int AttemptNumber { get; private set; }

    public void AcceptOutcome()
    {
        if (ExecutionTaskSafe?.IsCompleted == true)
        {
            IsAccepted = true;
        }
        else
        {
            throw new InvalidOperationException("Unable to accept outcome for a task that is not completed.");
        }
    }

    public void Cancel()
    {
        if (!IsAccepted)
        {
            _cancellationSource!.Cancel();
        }
    }

    public async ValueTask<bool> InitializeAsync<TState>(
        HedgedTaskType type,
        ResilienceContext primaryContext,
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> primaryCallback,
        TState state,
        int attemptNumber)
    {
        AttemptNumber = attemptNumber;
        Type = type;
        _cancellationSource = _cancellationTokenSourcePool.Get(System.Threading.Timeout.InfiniteTimeSpan);
        _startExecutionTimestamp = _timeProvider.GetTimestamp();
        _activeContext = _cachedContext;
        _activeContext.InitializeFrom(primaryContext, _cancellationSource!.Token);

#if NET
        _cancellationRegistration = primaryContext.CancellationToken.UnsafeRegister(static o => ((CancellationTokenSource)o!).Cancel(), _cancellationSource);
#else
        _cancellationRegistration = primaryContext.CancellationToken.Register(static o => ((CancellationTokenSource)o!).Cancel(), _cancellationSource);
#endif

        if (type == HedgedTaskType.Secondary)
        {
            Func<ValueTask<Outcome<T>>>? action = null;
            if (!_handler.IsDefaultActionGenerator)
            {
                try
                {
                    action = _handler.ActionGenerator(CreateArguments(primaryCallback, primaryContext, state, attemptNumber));
                    if (action == null)
                    {
                        await ResetAsync().ConfigureAwait(false);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    _stopExecutionTimestamp = _timeProvider.GetTimestamp();
                    ExecutionTaskSafe = UpdateOutcomeAsync(new(e));
                    return true;
                }
            }

            var args = new OnHedgingArguments<T>(primaryContext, Context, attemptNumber - 1);
            _telemetry.Report(new(ResilienceEventSeverity.Warning, HedgingConstants.OnHedgingEventName), Context, args);

            if (_handler.OnHedging is { } onHedging)
            {
                await onHedging(args).ConfigureAwait(Context.ContinueOnCapturedContext);
            }

            ExecutionTaskSafe = ExecuteSecondaryActionAsync(action, primaryCallback, state, primaryContext.IsSynchronous);
        }
        else
        {
            ExecutionTaskSafe = ExecutePrimaryActionAsync(primaryCallback, state);
        }

        return true;
    }

    private HedgingActionGeneratorArguments<T> CreateArguments<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> primaryCallback,
        ResilienceContext primaryContext,
        TState state,
        int attempt) => new(primaryContext, Context, attempt, (context) => primaryCallback(context, state));

    public async ValueTask ResetAsync()
    {
        OnReset?.Invoke(this);

#pragma warning disable CA1849, S6966 // Call async methods when in an async method, OK here as the callback is synchronous
        _cancellationRegistration.Dispose();
        _cancellationRegistration = default;
#pragma warning restore CA1849, S6966

        if (!IsAccepted)
        {
            await DisposeHelper.TryDisposeSafeAsync(Outcome.Result!, Context.IsSynchronous).ConfigureAwait(false);

            // not accepted executions are always cancelled, so the cancellation source must be
            // disposed instead of returning it to the pool
            _cancellationSource!.Dispose();
        }
        else
        {
            // accepted outcome means that the cancellation source can be be returned to the pool
            // since it was most likely not cancelled
            _cancellationTokenSourcePool.Return(_cancellationSource!);
        }

        IsAccepted = false;
        Outcome = default;
        IsHandled = false;
        OnReset = null;
        AttemptNumber = 0;
        _activeContext = null;
        _cachedContext.Reset();
        _cancellationSource = null!;
        _startExecutionTimestamp = 0;
        _stopExecutionTimestamp = 0;
    }

    [DebuggerDisableUserUnhandledExceptions]
    private async Task ExecuteSecondaryActionAsync<TState>(
        Func<ValueTask<Outcome<T>>>? action,
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> primaryCallback,
        TState state,
        bool isSynchronous)
    {
        Outcome<T> outcome;

        try
        {
            var task = action?.Invoke() ?? (isSynchronous ? ExecuteSecondaryActionSync(primaryCallback, state) : primaryCallback(Context, state));
            outcome = await task.ConfigureAwait(Context.ContinueOnCapturedContext);
        }
        catch (Exception e)
        {
            outcome = new(e);
        }

        _stopExecutionTimestamp = _timeProvider.GetTimestamp();
        await UpdateOutcomeAsync(outcome).ConfigureAwait(Context.ContinueOnCapturedContext);
    }

    private ValueTask<Outcome<T>> ExecuteSecondaryActionSync<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> primaryCallback, TState state)
        => new(Task.Run(() => primaryCallback(Context, state).AsTask()));

    [DebuggerDisableUserUnhandledExceptions]
    private async Task ExecutePrimaryActionAsync<TState>(Func<ResilienceContext, TState, ValueTask<Outcome<T>>> primaryCallback, TState state)
    {
        Outcome<T> outcome;

        try
        {
            outcome = await primaryCallback(Context, state).ConfigureAwait(Context.ContinueOnCapturedContext);
        }
        catch (Exception e)
        {
            outcome = new(e);
        }

        _stopExecutionTimestamp = _timeProvider.GetTimestamp();
        await UpdateOutcomeAsync(outcome).ConfigureAwait(Context.ContinueOnCapturedContext);
    }

    private async Task UpdateOutcomeAsync(Outcome<T> outcome)
    {
        var args = new HedgingPredicateArguments<T>(Context, outcome, AttemptNumber);
        Outcome = outcome;
        IsHandled = await _handler.ShouldHandle(args).ConfigureAwait(Context.ContinueOnCapturedContext);
        TelemetryUtil.ReportExecutionAttempt(_telemetry, Context, outcome, AttemptNumber, ExecutionTime, IsHandled);
    }
}
