using Polly.Hedging.Utils;
using Polly.Telemetry;

namespace Polly.Hedging;

internal sealed class HedgingResilienceStrategy : ResilienceStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly HedgingController _controller;

    public HedgingResilienceStrategy(
        TimeSpan hedgingDelay,
        int maxHedgedAttempts,
        HedgingHandler.Handler hedgingHandler,
        EventInvoker<OnHedgingArguments>? onHedging,
        Func<HedgingDelayArguments, ValueTask<TimeSpan>>? hedgingDelayGenerator,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry)
    {
        HedgingDelay = hedgingDelay;
        MaxHedgedAttempts = maxHedgedAttempts;
        HedgingDelayGenerator = hedgingDelayGenerator;
        HedgingHandler = hedgingHandler;
        OnHedging = onHedging;

        _telemetry = telemetry;
        _controller = new HedgingController(timeProvider, HedgingHandler, maxHedgedAttempts);
    }

    public TimeSpan HedgingDelay { get; }

    public int MaxHedgedAttempts { get; }

    public Func<HedgingDelayArguments, ValueTask<TimeSpan>>? HedgingDelayGenerator { get; }

    public HedgingHandler.Handler HedgingHandler { get; }

    public EventInvoker<OnHedgingArguments>? OnHedging { get; }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (!HedgingHandler.HandlesHedging<TResult>())
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var continueOnCapturedContext = context.ContinueOnCapturedContext;
        var cancellationToken = context.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        // create hedging execution context
        var hedgingContext = _controller.GetContext(context);

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ((await hedgingContext.LoadExecutionAsync(callback, state).ConfigureAwait(context.ContinueOnCapturedContext)).Outcome is Outcome<TResult> outcome)
                {
                    return outcome;
                }

                var delay = await GetHedgingDelayAsync(context, hedgingContext.LoadedTasks).ConfigureAwait(continueOnCapturedContext);
                var execution = await hedgingContext.TryWaitForCompletedExecutionAsync(delay).ConfigureAwait(continueOnCapturedContext);
                if (execution is null)
                {
                    // If completedHedgedTask is null it indicates that we still do not have any finished hedged task within the hedging delay.
                    // We will create additional hedged task in the next iteration.
                    continue;
                }

                outcome = execution.Outcome.AsOutcome<TResult>();

                if (!execution.IsHandled)
                {
                    execution.AcceptOutcome();
                    return outcome;
                }

                var onHedgingArgs = new OutcomeArguments<TResult, OnHedgingArguments>(context, outcome, new OnHedgingArguments(hedgingContext.LoadedTasks - 1));
                _telemetry.Report(HedgingConstants.OnHedgingEventName, onHedgingArgs);

                if (OnHedging is not null)
                {
                    // If nothing has been returned or thrown yet, the result is a transient failure,
                    // and other hedged request will be awaited.
                    // Before it, one needs to perform the task adjacent to each hedged call.
                    await OnHedging.HandleAsync(onHedgingArgs).ConfigureAwait(continueOnCapturedContext);
                }
            }
        }
        finally
        {
            hedgingContext.Complete();
        }
    }

    internal ValueTask<TimeSpan> GetHedgingDelayAsync(ResilienceContext context, int attempt)
    {
        if (HedgingDelayGenerator == null)
        {
            return new ValueTask<TimeSpan>(HedgingDelay);
        }

        return HedgingDelayGenerator(new HedgingDelayArguments(context, attempt));
    }
}
