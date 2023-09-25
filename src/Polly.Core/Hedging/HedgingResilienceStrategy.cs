using System.Diagnostics.CodeAnalysis;
using Polly.Hedging.Controller;
using Polly.Hedging.Utils;
using Polly.Telemetry;

namespace Polly.Hedging;

internal sealed class HedgingResilienceStrategy<T> : ResilienceStrategy<T>
{
    private readonly HedgingController<T> _controller;

    public HedgingResilienceStrategy(
        TimeSpan hedgingDelay,
        int maxHedgedAttempts,
        HedgingHandler<T> hedgingHandler,
        Func<HedgingDelayGeneratorArguments, ValueTask<TimeSpan>>? hedgingDelayGenerator,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry)
    {
        HedgingDelay = hedgingDelay;
        TotalAttempts = maxHedgedAttempts + 1; // include the initial attempt
        DelayGenerator = hedgingDelayGenerator;
        HedgingHandler = hedgingHandler;
        _controller = new HedgingController<T>(telemetry, timeProvider, HedgingHandler, TotalAttempts);
    }

    public TimeSpan HedgingDelay { get; }

    public int TotalAttempts { get; }

    public Func<HedgingDelayGeneratorArguments, ValueTask<TimeSpan>>? DelayGenerator { get; }

    public HedgingHandler<T> HedgingHandler { get; }

    [ExcludeFromCodeCoverage] // coverlet issue
    protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state)
    {
        // create hedging execution context
        var hedgingContext = _controller.GetContext(context);

        try
        {
            return await ExecuteCoreAsync(hedgingContext, callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        finally
        {
            await hedgingContext.DisposeAsync().ConfigureAwait(context.ContinueOnCapturedContext);
        }
    }

    private async ValueTask<Outcome<T>> ExecuteCoreAsync<TState>(
        HedgingExecutionContext<T> hedgingContext,
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state)
    {
        // Capture the original cancellation token so it stays the same while hedging is executing.
        // If we do not do this the inner strategy can replace the cancellation token and with the concurrent
        // nature of hedging this can cause issues.
        var cancellationToken = context.CancellationToken;
        var continueOnCapturedContext = context.ContinueOnCapturedContext;

        var attempt = -1;

        while (true)
        {
            attempt++;
            if (cancellationToken.IsCancellationRequested)
            {
                return Outcome.FromException<T>(new OperationCanceledException(cancellationToken).TrySetStackTrace());
            }

            var loadedExecution = await hedgingContext.LoadExecutionAsync(callback, state).ConfigureAwait(context.ContinueOnCapturedContext);

            if (loadedExecution.Outcome is Outcome<T> outcome)
            {
                return outcome;
            }

            var delay = await GetHedgingDelayAsync(context, hedgingContext.LoadedTasks).ConfigureAwait(continueOnCapturedContext);
            var execution = await hedgingContext.TryWaitForCompletedExecutionAsync(delay).ConfigureAwait(continueOnCapturedContext);
            if (execution is null)
            {
                continue;
            }

            outcome = execution.Outcome;

            if (!execution.IsHandled)
            {
                execution.AcceptOutcome();
                return outcome;
            }
        }
    }

    internal ValueTask<TimeSpan> GetHedgingDelayAsync(ResilienceContext context, int attempt)
    {
        if (DelayGenerator == null)
        {
            return new ValueTask<TimeSpan>(HedgingDelay);
        }

        return DelayGenerator(new HedgingDelayGeneratorArguments(context, attempt));
    }
}
