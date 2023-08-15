using Polly.Utils;

namespace Polly.Testing;

/// <summary>
/// The test-related extensions for <see cref="ResiliencePipeline"/> and <see cref="ResiliencePipeline{TResult}"/>.
/// </summary>
public static class ResiliencePipelineExtensions
{
    /// <summary>
    /// Gets the pipeline descriptor.
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <param name="pipeline">The pipeline instance.</param>
    /// <returns>A pipeline descriptor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/> is <see langword="null"/>.</exception>
    public static ResiliencePipelineDescriptor GetPipelineDescriptor<TResult>(this ResiliencePipeline<TResult> pipeline)
    {
        Guard.NotNull(pipeline);

        return GetPipelineDescriptorCore<TResult>(pipeline.Strategy);
    }

    /// <summary>
    /// Gets the pipeline descriptor.
    /// </summary>
    /// <param name="pipeline">The pipeline instance.</param>
    /// <returns>A pipeline descriptor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/> is <see langword="null"/>.</exception>
    public static ResiliencePipelineDescriptor GetPipelineDescriptor(this ResiliencePipeline pipeline)
    {
        Guard.NotNull(pipeline);

        return GetPipelineDescriptorCore<object>(pipeline);
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
