using Polly.Utils;

namespace Polly.Testing;

/// <summary>
/// The test-related extensions for <see cref="ResiliencePipeline"/> and <see cref="ResiliencePipeline{TResult}"/>.
/// </summary>
public static class ResiliencePipelineExtensions
{
    /// <summary>
    /// Gets the inner strategies the <paramref name="strategy"/> is composed of.
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>A list of inner strategies.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static ResiliencePipelineDescriptor GetPipelineDescriptor<TResult>(this ResiliencePipeline<TResult> strategy)
    {
        Guard.NotNull(strategy);

        return GetPipelineDescriptorCore<TResult>(strategy.Strategy);
    }

    /// <summary>
    /// Gets the inner strategies the <paramref name="strategy"/> is composed of.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>A list of inner strategies.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static ResiliencePipelineDescriptor GetPipelineDescriptor(this ResiliencePipeline strategy)
    {
        Guard.NotNull(strategy);

        return GetPipelineDescriptorCore<object>(strategy);
    }

    private static ResiliencePipelineDescriptor GetPipelineDescriptorCore<T>(ResiliencePipeline strategy)
    {
        var strategies = new List<ResiliencePipeline>();
        strategy.ExpandStrategies(strategies);

        var innerStrategies = strategies.Select(s => new ResilienceStrategyDescriptor(s.Options, GetStrategyInstance<T>(s))).ToList();

        return new ResiliencePipelineDescriptor(
            innerStrategies.Where(s => !ShouldSkip(s.StrategyInstance)).ToList().AsReadOnly(),
            isReloadable: innerStrategies.Exists(s => s.StrategyInstance is ReloadableResiliencePipeline));
    }

    private static object GetStrategyInstance<T>(ResiliencePipeline strategy)
    {
        if (strategy is ResiliencePipelineBridge<T> reactiveBridge)
        {
            return reactiveBridge.Strategy;
        }

        if (strategy is ResiliencePipelineBridge nonReactiveBridge)
        {
            return nonReactiveBridge.Strategy;
        }

        return strategy;
    }

    private static bool ShouldSkip(object instance) => instance is ReloadableResiliencePipeline;

    private static void ExpandStrategies(this ResiliencePipeline strategy, List<ResiliencePipeline> strategies)
    {
        if (strategy is CompositeResiliencePipeline pipeline)
        {
            foreach (var inner in pipeline.Strategies)
            {
                inner.ExpandStrategies(strategies);
            }
        }
        else if (strategy is ReloadableResiliencePipeline reloadable)
        {
            strategies.Add(reloadable);
            ExpandStrategies(reloadable.Pipeline, strategies);
        }
        else
        {
            strategies.Add(strategy);
        }
    }
}
