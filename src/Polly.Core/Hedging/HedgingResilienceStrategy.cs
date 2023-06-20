using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Polly.Hedging.Utils;
using Polly.Telemetry;

namespace Polly.Hedging;

internal sealed class HedgingResilienceStrategy<T> : ResilienceStrategy
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly HedgingController<T> _controller;

    public HedgingResilienceStrategy(
        TimeSpan hedgingDelay,
        int maxHedgedAttempts,
        HedgingHandler<T> hedgingHandler,
        EventInvoker<OnHedgingArguments>? onHedging,
        Func<HedgingDelayArguments, ValueTask<TimeSpan>>? hedgingDelayGenerator,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry)
    {
        HedgingDelay = hedgingDelay;
        MaxHedgedAttempts = maxHedgedAttempts;
        HedgingDelayGenerator = hedgingDelayGenerator;
        _timeProvider = timeProvider;
        HedgingHandler = hedgingHandler;
        OnHedging = onHedging;

        _telemetry = telemetry;
        _controller = new HedgingController<T>(telemetry, timeProvider, HedgingHandler, maxHedgedAttempts);
    }

    public TimeSpan HedgingDelay { get; }

    public int MaxHedgedAttempts { get; }

    public Func<HedgingDelayArguments, ValueTask<TimeSpan>>? HedgingDelayGenerator { get; }

    public HedgingHandler<T> HedgingHandler { get; }

    public EventInvoker<OnHedgingArguments>? OnHedging { get; }

    [ExcludeFromCodeCoverage] // coverlet issue
    protected internal override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (!HedgingHandler.HandlesHedging<TResult>())
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

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

    private async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        HedgingExecutionContext<T> hedgingContext,
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var attempt = -1;
        while (true)
        {
            attempt++;
            var continueOnCapturedContext = context.ContinueOnCapturedContext;
            var cancellationToken = context.CancellationToken;
            var start = _timeProvider.GetTimestamp();
            if (cancellationToken.IsCancellationRequested)
            {
                return new Outcome<TResult>(new OperationCanceledException(cancellationToken).TrySetStackTrace());
            }

            var loadedExecution = await hedgingContext.LoadExecutionAsync(callback, state).ConfigureAwait(context.ContinueOnCapturedContext);

            if (loadedExecution.Outcome is Outcome<TResult> outcome)
            {
                return outcome;
            }

            var delay = await GetHedgingDelayAsync(context, hedgingContext.LoadedTasks).ConfigureAwait(continueOnCapturedContext);
            var execution = await hedgingContext.TryWaitForCompletedExecutionAsync(delay).ConfigureAwait(continueOnCapturedContext);
            if (execution is null)
            {
                // If completedHedgedTask is null it indicates that we still do not have any finished hedged task within the hedging delay.
                // We will create additional hedged task in the next iteration.
                await HandleOnHedgingAsync(
                    context,
                    new Outcome<TResult>(default(TResult)),
                    new OnHedgingArguments(attempt, HasOutcome: false, ExecutionTime: delay)).ConfigureAwait(context.ContinueOnCapturedContext);
                continue;
            }

            outcome = execution.Outcome.AsOutcome<TResult>();

            if (!execution.IsHandled)
            {
                execution.AcceptOutcome();
                return outcome;
            }

            var executionTime = _timeProvider.GetElapsedTime(start);
            await HandleOnHedgingAsync(
                context,
                outcome,
                new OnHedgingArguments(attempt, HasOutcome: true, executionTime)).ConfigureAwait(context.ContinueOnCapturedContext);
        }
    }

    private async ValueTask HandleOnHedgingAsync<TResult>(ResilienceContext context, Outcome<TResult> outcome, OnHedgingArguments args)
    {
        var onHedgingArgs = new OutcomeArguments<TResult, OnHedgingArguments>(
            context,
            outcome,
            args);

        _telemetry.Report(HedgingConstants.OnHedgingEventName, onHedgingArgs);

        if (OnHedging is not null)
        {
            // If nothing has been returned or thrown yet, the result is a transient failure,
            // and other hedged request will be awaited.
            // Before it, one needs to perform the task adjacent to each hedged call.
            await OnHedging.HandleAsync(onHedgingArgs).ConfigureAwait(context.ContinueOnCapturedContext);
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
