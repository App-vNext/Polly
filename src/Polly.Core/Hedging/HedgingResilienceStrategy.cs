using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using Polly.Hedging;
using Polly.Hedging.Utils;
using Polly.Strategy;
using Polly.Utils;

namespace Polly;

internal sealed class HedgingResilienceStrategy : ResilienceStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly HedgingController? _controller;

    public HedgingResilienceStrategy(HedgingStrategyOptions options, TimeProvider timeProvider, ResilienceStrategyTelemetry telemetry)
    {
        HedgingDelay = options.HedgingDelay;
        MaxHedgedAttempts = options.MaxHedgedAttempts;
        HedgingDelayGenerator = options.HedgingDelayGenerator.CreateHandler(HedgingConstants.DefaultHedgingDelay, static _ => true);
        HedgingHandler = options.Handler.CreateHandler();
        OnHedgingHandler = options.OnHedging.CreateHandler();

        _telemetry = telemetry;
        if (HedgingHandler != null)
        {
            _controller = new HedgingController(timeProvider, HedgingHandler, options.MaxHedgedAttempts);
        }
    }

    public TimeSpan HedgingDelay { get; }

    public int MaxHedgedAttempts { get; }

    public Func<HedgingDelayArguments, ValueTask<TimeSpan>>? HedgingDelayGenerator { get; }

    public HedgingHandler.Handler? HedgingHandler { get; }

    public OutcomeEvent<OnHedgingArguments>.Handler? OnHedgingHandler { get; }

    protected internal override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        if (_controller == null || !HedgingHandler!.HandlesHedging<TResult>())
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var continueOnCapturedContext = context.ContinueOnCapturedContext;
        context.CancellationToken.ThrowIfCancellationRequested();

        // create hedging execution context
        var hedgingContext = _controller.GetContext(context);

        try
        {
            while (true)
            {
                if ((await hedgingContext.LoadExecutionAsync(callback, state).ConfigureAwait(context.ContinueOnCapturedContext)).Outcome is Outcome<TResult> outcome)
                {
                    return HandleOutcome(outcome);
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
                    return HandleOutcome(outcome);
                }

                var onHedgingArgs = new OnHedgingArguments(context, hedgingContext.LoadedTasks - 1);
                _telemetry.Report(HedgingConstants.OnHedgingEventName, outcome, onHedgingArgs);

                if (OnHedgingHandler != null)
                {
                    // If nothing has been returned or thrown yet, the result is a transient failure,
                    // and other hedged request will be awaited.
                    // Before it, one needs to perform the task adjacent to each hedged call.
                    await OnHedgingHandler.HandleAsync(outcome, onHedgingArgs).ConfigureAwait(continueOnCapturedContext);
                }
            }
        }
        finally
        {
            hedgingContext.Complete();
        }
    }

    [ExcludeFromCodeCoverage]
    private static TResult HandleOutcome<TResult>(Outcome<TResult> outcome)
    {
        if (outcome.Exception is not null)
        {
            ExceptionDispatchInfo.Capture(outcome.Exception).Throw();
        }

        return outcome.Result!;
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
