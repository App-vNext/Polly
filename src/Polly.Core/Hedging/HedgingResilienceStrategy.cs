using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Polly.Hedging.Utils;
using Polly.Telemetry;

namespace Polly.Hedging;

internal sealed class HedgingResilienceStrategy<T> : OutcomeResilienceStrategy<T>
{
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly HedgingController<T> _controller;

    public HedgingResilienceStrategy(
        TimeSpan hedgingDelay,
        int maxHedgedAttempts,
        HedgingHandler<T> hedgingHandler,
        Func<OutcomeArguments<T, OnHedgingArguments>, ValueTask>? onHedging,
        Func<HedgingDelayArguments, ValueTask<TimeSpan>>? hedgingDelayGenerator,
        TimeProvider timeProvider,
        ResilienceStrategyTelemetry telemetry,
        bool isGeneric)
        : base(isGeneric)
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

    public Func<OutcomeArguments<T, OnHedgingArguments>, ValueTask>? OnHedging { get; }

    [ExcludeFromCodeCoverage] // coverlet issue
    protected override async ValueTask<Outcome<T>> ExecuteCallbackAsync<TState>(
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
        var attempt = -1;
        while (true)
        {
            attempt++;
            var continueOnCapturedContext = context.ContinueOnCapturedContext;
            var cancellationToken = context.CancellationToken;
            var start = _timeProvider.GetTimestamp();
            if (cancellationToken.IsCancellationRequested)
            {
                return new Outcome<T>(new OperationCanceledException(cancellationToken).TrySetStackTrace());
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
                // If completedHedgedTask is null it indicates that we still do not have any finished hedged task within the hedging delay.
                // We will create additional hedged task in the next iteration.
                await HandleOnHedgingAsync(
                    context,
                    new Outcome<T>(default(T)),
                    new OnHedgingArguments(attempt, HasOutcome: false, ExecutionTime: delay)).ConfigureAwait(context.ContinueOnCapturedContext);
                continue;
            }

            outcome = execution.Outcome.AsOutcome<T>();

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

    private async ValueTask HandleOnHedgingAsync(ResilienceContext context, Outcome<T> outcome, OnHedgingArguments args)
    {
        var onHedgingArgs = new OutcomeArguments<T, OnHedgingArguments>(
            context,
            outcome,
            args);

        _telemetry.Report(HedgingConstants.OnHedgingEventName, onHedgingArgs);

        if (OnHedging is not null)
        {
            // If nothing has been returned or thrown yet, the result is a transient failure,
            // and other hedged request will be awaited.
            // Before it, one needs to perform the task adjacent to each hedged call.
            await OnHedging(onHedgingArgs).ConfigureAwait(context.ContinueOnCapturedContext);
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
