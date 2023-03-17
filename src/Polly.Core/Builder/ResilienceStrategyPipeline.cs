using System;

namespace Polly.Builder;

/// <summary>
/// A pipeline of strategies.
/// </summary>
internal sealed class ResilienceStrategyPipeline : DelegatingResilienceStrategy
{
    private readonly ResilienceStrategy _pipeline;

    public static ResilienceStrategyPipeline CreatePipelineAndFreezeStrategies(IReadOnlyList<ResilienceStrategy> strategies)
    {
        Guard.NotNull(strategies);

        if (strategies.Count < 2)
        {
#pragma warning disable S2302 // "nameof" should be used
            throw new InvalidOperationException("The resilience pipeline must contain at least two resilience strategies.");
#pragma warning restore S2302 // "nameof" should be used
        }

        if (strategies.Distinct().Count() != strategies.Count)
        {
#pragma warning disable S2302 // "nameof" should be used
            throw new InvalidOperationException("The resilience pipeline must contain unique resilience strategies.");
#pragma warning restore S2302 // "nameof" should be used
        }

        var delegatingStrategies = strategies.Select(strategy =>
        {
            if (strategy is DelegatingResilienceStrategy delegatingStrategy)
            {
                return delegatingStrategy;
            }
            else
            {
                return new DelegatingStrategyWrapper(strategy);
            }
        }).ToList();

        for (var i = 0; i < delegatingStrategies.Count - 1; i++)
        {
            delegatingStrategies[i].Next = delegatingStrategies[i + 1];
        }

        // now, freeze the strategies so any further modifications are not allowed
        foreach (var strategy in delegatingStrategies)
        {
            strategy.Freeze();
        }

        return new ResilienceStrategyPipeline(delegatingStrategies[0], strategies);
    }

    private ResilienceStrategyPipeline(ResilienceStrategy pipeline, IReadOnlyList<ResilienceStrategy> strategies)
    {
        Strategies = strategies;
        _pipeline = pipeline;
    }

    public IReadOnlyList<ResilienceStrategy> Strategies { get; }

    protected internal override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        return _pipeline.ExecuteCoreAsync(
            static (context, state) => state.Next.ExecuteCoreAsync(state.callback, context, state.state),
            context,
            (Next, callback, state));
    }

    /// <summary>
    /// A wrapper that converts a <see cref="ResilienceStrategy"/> into a <see cref="DelegatingResilienceStrategy"/>.
    /// </summary>
    private sealed class DelegatingStrategyWrapper : DelegatingResilienceStrategy
    {
        private readonly ResilienceStrategy _strategy;

        public DelegatingStrategyWrapper(ResilienceStrategy strategy) => _strategy = strategy;

        protected internal override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
        {
            return _strategy.ExecuteCoreAsync(
                static (context, state) => state.Next.ExecuteCoreAsync(state.callback, context, state.state),
                context,
                (Next, callback, state));
        }
    }
}
