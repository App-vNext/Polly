using Polly.Hedging.Utils;

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
    private readonly ResilienceContext _cachedContext = ResilienceContext.Get();
    private readonly CancellationTokenSourcePool _cancellationTokenSourcePool;
    private readonly HedgingHandler<T> _handler;
    private CancellationTokenSource? _cancellationSource;
    private CancellationTokenRegistration? _cancellationRegistration;
    private ResilienceContext? _activeContext;

    public TaskExecution(HedgingHandler<T> handler, CancellationTokenSourcePool cancellationTokenSourcePool)
    {
        _handler = handler;
        _cancellationTokenSourcePool = cancellationTokenSourcePool;
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

    public Outcome<object> Outcome { get; private set; }

    public bool IsHandled { get; private set; }

    public bool IsAccepted { get; private set; }

    public ResilienceProperties Properties { get; } = new();

    public ResilienceContext Context => _activeContext ?? throw new InvalidOperationException("TaskExecution is not initialized.");

    public HedgedTaskType Type { get; set; }

    public Action<TaskExecution<T>>? OnReset { get; set; }

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

    public async ValueTask<bool> InitializeAsync<TResult, TState>(
        HedgedTaskType type,
        ContextSnapshot snapshot,
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> primaryCallback,
        TState state,
        int attempt)
    {
        Type = type;
        _cancellationSource = _cancellationTokenSourcePool.Get(System.Threading.Timeout.InfiniteTimeSpan);
        Properties.Replace(snapshot.OriginalProperties);

        if (snapshot.OriginalCancellationToken.CanBeCanceled)
        {
            _cancellationRegistration = snapshot.OriginalCancellationToken.Register(o => ((CancellationTokenSource)o!).Cancel(), _cancellationSource);
        }

        PrepareContext(ref snapshot);

        if (type == HedgedTaskType.Secondary)
        {
            Func<ValueTask<Outcome<TResult>>>? action = null;

            try
            {
                action = _handler.GenerateAction(CreateArguments(primaryCallback, state, attempt));
                if (action == null)
                {
                    await ResetAsync().ConfigureAwait(false);
                    return false;
                }
            }
            catch (Exception e)
            {
                ExecutionTaskSafe = ExecuteCreateActionException<TResult>(e);
                return true;
            }

            ExecutionTaskSafe = ExecuteSecondaryActionAsync(action);
        }
        else
        {
            ExecutionTaskSafe = ExecutePrimaryActionAsync(primaryCallback, state);
        }

        return true;
    }

    private HedgingActionGeneratorArguments<TResult> CreateArguments<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> primaryCallback,
        TState state,
        int attempt) => new(Context, attempt, (context) => primaryCallback(context, state));

    public async ValueTask ResetAsync()
    {
        OnReset?.Invoke(this);

        if (_cancellationRegistration is CancellationTokenRegistration registration)
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
        Properties.Clear();
        OnReset = null;
        _activeContext = null;
        _cachedContext.Reset();
        _cancellationSource = null!;
    }

    private async Task ExecuteSecondaryActionAsync<TResult>(Func<ValueTask<Outcome<TResult>>> action)
    {
        Outcome<TResult> outcome;

        try
        {
            outcome = await action().ConfigureAwait(Context.ContinueOnCapturedContext);
        }
        catch (Exception e)
        {
            outcome = new Outcome<TResult>(e);
        }

        await UpdateOutcomeAsync(outcome).ConfigureAwait(Context.ContinueOnCapturedContext);
    }

    private async Task ExecuteCreateActionException<TResult>(Exception e)
    {
        await UpdateOutcomeAsync(new Outcome<TResult>(e)).ConfigureAwait(Context.ContinueOnCapturedContext);
    }

    private async Task ExecutePrimaryActionAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> primaryCallback, TState state)
    {
        var outcome = await primaryCallback(Context, state).ConfigureAwait(Context.ContinueOnCapturedContext);
        await UpdateOutcomeAsync(outcome).ConfigureAwait(Context.ContinueOnCapturedContext);
    }

    private async Task UpdateOutcomeAsync<TResult>(Outcome<TResult> outcome)
    {
        var args = new OutcomeArguments<TResult, HandleHedgingArguments>(Context, outcome, new HandleHedgingArguments());
        Outcome = outcome.AsOutcome();
        IsHandled = await _handler.ShouldHandle.HandleAsync(args).ConfigureAwait(Context.ContinueOnCapturedContext);
    }

    private void PrepareContext(ref ContextSnapshot snapshot)
    {
        if (Type == HedgedTaskType.Primary)
        {
            // now just replace the properties
            _activeContext = snapshot.Context;
        }
        else
        {
            // secondary hedged tasks get their own unique context
            _activeContext = _cachedContext;
            _activeContext.InitializeFrom(snapshot.Context);
        }

        _activeContext.Properties = Properties;
        _activeContext.CancellationToken = _cancellationSource!.Token;
    }
}
