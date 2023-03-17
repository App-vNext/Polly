using System;

namespace Polly.Builder;

/// <summary>
/// A pipeline of strategies.
/// </summary>
internal sealed class ResilienceStrategyPipeline : DelegatingResilienceStrategy
{
    private readonly IResilienceStrategy _strategy;

    public static ResilienceStrategyPipeline CreateAndFreezeStrategies(IReadOnlyList<DelegatingResilienceStrategy> strategies)
    {
        Guard.NotNull(strategies);

        if (strategies.Count < 2)
        {
            throw new ArgumentException("The pipeline must contain at least two strategies.", nameof(strategies));
        }

        for (var i = 0; i < strategies.Count - 1; i++)
        {
            strategies[i].Next = strategies[i + 1];
        }

        // now, freeze the strategies so any further modifications are not allowed
        foreach (var strategy in strategies)
        {
            strategy.Freeze();
        }

        return new ResilienceStrategyPipeline(strategies);
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
