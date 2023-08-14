using Polly.Telemetry;

namespace Polly.Utils;

#pragma warning disable S2302 // "nameof" should be used

/// <summary>
/// A combination of multiple resilience strategies.
/// </summary>
[DebuggerDisplay("CompositeResiliencePipeline, Strategies = {Strategies.Count}")]
[DebuggerTypeProxy(typeof(DebuggerProxy))]
internal sealed partial class CompositeResiliencePipeline : ResiliencePipeline
{
    private readonly ResiliencePipeline _firstStrategy;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly TimeProvider _timeProvider;

    public static CompositeResiliencePipeline Create(IReadOnlyList<ResiliencePipeline> strategies, ResilienceStrategyTelemetry telemetry, TimeProvider timeProvider)
    {
        Guard.NotNull(strategies);

        if (strategies.Count == 0)
        {
            throw new InvalidOperationException("The composite resilience strategy must contain at least one resilience strategy.");
        }

        if (strategies.Distinct().Count() != strategies.Count)
        {
            throw new InvalidOperationException("The composite resilience strategy must contain unique resilience strategies.");
        }

        if (strategies.Count == 1)
        {
            return new CompositeResiliencePipeline(strategies[0], strategies, telemetry, timeProvider);
        }

        // convert all strategies to delegating ones (except the last one as it's not required)
        var delegatingStrategies = strategies
            .Take(strategies.Count - 1)
            .Select(strategy => new DelegatingResiliencePipeline(strategy))
            .ToList();

#if NET6_0_OR_GREATER
        // link the last one
        delegatingStrategies[^1].Next = strategies[^1];
#else
        delegatingStrategies[delegatingStrategies.Count - 1].Next = strategies[strategies.Count - 1];
#endif

        // link the remaining ones
        for (var i = 0; i < delegatingStrategies.Count - 1; i++)
        {
            delegatingStrategies[i].Next = delegatingStrategies[i + 1];
        }

        return new CompositeResiliencePipeline(delegatingStrategies[0], strategies, telemetry, timeProvider);
    }

    private CompositeResiliencePipeline(ResiliencePipeline first, IReadOnlyList<ResiliencePipeline> strategies, ResilienceStrategyTelemetry telemetry, TimeProvider timeProvider)
    {
        Strategies = strategies;

        _telemetry = telemetry;
        _timeProvider = timeProvider;
        _firstStrategy = first;
    }

    public IReadOnlyList<ResiliencePipeline> Strategies { get; }

    internal override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var timeStamp = _timeProvider.GetTimestamp();
        _telemetry.Report(new ResilienceEvent(ResilienceEventSeverity.Debug, TelemetryUtil.PipelineExecuting), context, PipelineExecutingArguments.Instance);

        Outcome<TResult> outcome;

        if (context.CancellationToken.IsCancellationRequested)
        {
            outcome = Outcome.FromException<TResult>(new OperationCanceledException(context.CancellationToken).TrySetStackTrace());
        }
        else
        {
            outcome = await _firstStrategy.ExecuteCore(callback, context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var durationArgs = PipelineExecutedArguments.Get(_timeProvider.GetElapsedTime(timeStamp));
        _telemetry.Report(
            new ResilienceEvent(ResilienceEventSeverity.Information, TelemetryUtil.PipelineExecuted),
            new OutcomeArguments<TResult, PipelineExecutedArguments>(context, outcome, durationArgs));
        PipelineExecutedArguments.Return(durationArgs);

        return outcome;
    }

    /// <summary>
    /// A resilience strategy that delegates the execution to the next strategy in the chain.
    /// </summary>
    private sealed class DelegatingResiliencePipeline : ResiliencePipeline
    {
        private readonly ResiliencePipeline _pipeline;

        public DelegatingResiliencePipeline(ResiliencePipeline strategy) => _pipeline = strategy;

        public ResiliencePipeline? Next { get; set; }

        internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            return _pipeline.ExecuteCore(
                static (context, state) =>
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        return Outcome.FromExceptionAsTask<TResult>(new OperationCanceledException(context.CancellationToken).TrySetStackTrace());
                    }

                    return state.Next!.ExecuteCore(state.callback, context, state.state);
                },
                context,
                (Next, callback, state));
        }
    }
}
