using System;

namespace Polly.Strategy;

#pragma warning disable S2302 // "nameof" should be used

/// <summary>
/// A pipeline of strategies.
/// </summary>
internal sealed class ResilienceStrategyPipeline : ResilienceStrategy
{
    private readonly ResilienceStrategy _pipeline;

    public static ResilienceStrategyPipeline CreatePipeline(IReadOnlyList<ResilienceStrategy> strategies)
    {
        Guard.NotNull(strategies);

        if (strategies.Count < 2)
        {
            throw new InvalidOperationException("The resilience pipeline must contain at least two resilience strategies.");
        }

        if (strategies.Distinct().Count() != strategies.Count)
        {
            throw new InvalidOperationException("The resilience pipeline must contain unique resilience strategies.");
        }

        // convert all strategies to delegating ones (except the last one as it's not required)
        var delegatingStrategies = strategies
            .Take(strategies.Count - 1)
            .Select(strategy => new DelegatingResilienceStrategy(strategy))
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

        return new ResilienceStrategyPipeline(delegatingStrategies[0], strategies);
    }

    private ResilienceStrategyPipeline(ResilienceStrategy pipeline, IReadOnlyList<ResilienceStrategy> strategies)
    {
        Strategies = strategies;
        _pipeline = pipeline;
    }

    public IReadOnlyList<ResilienceStrategy> Strategies { get; }

    protected internal override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context, TState state)
    {
        return _pipeline.ExecuteCoreAsync(callback, context, state);
    }

    /// <summary>
    /// A resilience strategy that delegates the execution to the next strategy in the chain.
    /// </summary>
    private sealed class DelegatingResilienceStrategy : ResilienceStrategy
    {
        private readonly ResilienceStrategy _strategy;

        public DelegatingResilienceStrategy(ResilienceStrategy strategy) => _strategy = strategy;

        public ResilienceStrategy? Next { get; set; }

        protected internal override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            return _strategy.ExecuteCoreAsync(
                static (context, state) => state.Next!.ExecuteCoreAsync(state.callback, context, state.state),
                context,
                (Next, callback, state));
        }
    }
}
