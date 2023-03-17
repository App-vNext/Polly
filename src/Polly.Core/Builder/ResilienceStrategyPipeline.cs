using System;

namespace Polly.Builder;

/// <summary>
/// A pipeline of strategies.
/// </summary>
internal sealed class ResilienceStrategyPipeline : DelegatingResilienceStrategy
{
    private readonly IResilienceStrategy _strategy;

    public static ResilienceStrategyPipeline CreateAndFreezeStrategies(IReadOnlyList<IResilienceStrategy> strategies)
    {
        Guard.NotNull(strategies);

        if (strategies.Count < 2)
        {
            throw new ArgumentException("The pipeline must contain at least two strategies.", nameof(strategies));
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

        return new ResilienceStrategyPipeline(delegatingStrategies);
    }

    private ResilienceStrategyPipeline(IReadOnlyList<DelegatingResilienceStrategy> strategies)
    {
        Strategies = strategies;
        _strategy = strategies[0];
    }

    public IReadOnlyList<IResilienceStrategy> Strategies { get; }

    protected override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        return _strategy.ExecuteAsync(
            static (context, state) => state.Next.ExecuteAsync(state.callback, context, state.state),
            context,
            (Next, callback, state));
    }
}
