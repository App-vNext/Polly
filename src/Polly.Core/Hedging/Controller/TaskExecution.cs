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
    private CancellationTokenRegistration? _cancellationRegistration;
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

        if (primaryContext.CancellationToken.CanBeCanceled)
        {
            _cancellationRegistration = primaryContext.CancellationToken.Register(o => ((CancellationTokenSource)o!).Cancel(), _cancellationSource);
        }

        if (type == HedgedTaskType.Secondary)
        {
            Func<ValueTask<Outcome<T>>>? action = null;

            try
            {
                action = _handler.GenerateAction(CreateArguments(primaryCallback, primaryContext, state, attemptNumber));
                if (action == null)
                {
                    await ResetAsync().ConfigureAwait(false);
                    return false;
                }
            }
            catch (Exception e)
            {
                _stopExecutionTimestamp = _timeProvider.GetTimestamp();
                ExecutionTaskSafe = ExecuteCreateActionException(e);
                return true;
            }

            await HandleOnHedgingAsync(primaryContext, attemptNumber - 1).ConfigureAwait(Context.ContinueOnCapturedContext);

            ExecutionTaskSafe = ExecuteSecondaryActionAsync(action);
        }
        else
        {
            ExecutionTaskSafe = ExecutePrimaryActionAsync(primaryCallback, state);
        }

        return true;
    }

    private async Task HandleOnHedgingAsync(ResilienceContext primaryContext, int attemptNumber)
    {
        var args = new OnHedgingArguments<T>(
            primaryContext,
            Context,
            attemptNumber);

        _telemetry.Report(new(ResilienceEventSeverity.Warning, HedgingConstants.OnHedgingEventName), Context, args);

        if (_handler.OnHedging is { } onHedging)
        {
            await onHedging(args).ConfigureAwait(Context.ContinueOnCapturedContext);
        }
    }

    private HedgingActionGeneratorArguments<TResult> CreateArguments<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> primaryCallback,
        ResilienceContext primaryContext,
        TState state,
        int attempt) => new(primaryContext, Context, attempt, (context) => primaryCallback(context, state));

    public async ValueTask ResetAsync()
    {
        OnReset?.Invoke(this);

        if (_cancellationRegistration is { } registration)
        {
#if NETCOREAPP
            await registration.DisposeAsync().ConfigureAwait(false);
#else
            registration.Dispose();
#endif
        }

        _cancellationRegistration = null;

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
    private async Task ExecuteSecondaryActionAsync(Func<ValueTask<Outcome<T>>> action)
    {
        Outcome<T> outcome;

        try
        {
            outcome = await action().ConfigureAwait(Context.ContinueOnCapturedContext);
        }
        catch (Exception e)
        {
            outcome = Polly.Outcome.FromException<T>(e);
        }

        _stopExecutionTimestamp = _timeProvider.GetTimestamp();
        await UpdateOutcomeAsync(outcome).ConfigureAwait(Context.ContinueOnCapturedContext);
    }

    private async Task ExecuteCreateActionException(Exception e) => await UpdateOutcomeAsync(Polly.Outcome.FromException<T>(e)).ConfigureAwait(Context.ContinueOnCapturedContext);

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
            outcome = Polly.Outcome.FromException<T>(e);
        }

        _stopExecutionTimestamp = _timeProvider.GetTimestamp();
        await UpdateOutcomeAsync(outcome).ConfigureAwait(Context.ContinueOnCapturedContext);
    }

    private async Task UpdateOutcomeAsync(Outcome<T> outcome)
    {
        var args = new HedgingPredicateArguments<T>(Context, outcome);
        Outcome = outcome;
        IsHandled = await _handler.ShouldHandle(args).ConfigureAwait(Context.ContinueOnCapturedContext);
        TelemetryUtil.ReportExecutionAttempt(_telemetry, Context, outcome, AttemptNumber, ExecutionTime, IsHandled);
    }
}
