using Polly.Utils;
using Polly.Utils.Pipeline;

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

        return GetPipelineDescriptorCore<TResult>(pipeline.Component);
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

        return GetPipelineDescriptorCore<object>(pipeline.Component);
    }

    private static ResiliencePipelineDescriptor GetPipelineDescriptorCore<T>(PipelineComponent component)
    {
        var components = new List<PipelineComponent>();
        component.ExpandComponents(components);

        var descriptors = components.Select(s => new ResilienceStrategyDescriptor(s.Options, GetStrategyInstance<T>(s))).ToList();

        return new ResiliencePipelineDescriptor(
            descriptors.Where(s => !ShouldSkip(s.StrategyInstance)).ToList().AsReadOnly(),
            isReloadable: components.Exists(s => s is ReloadableComponent));
    }

    private static object GetStrategyInstance<T>(PipelineComponent component)
    {
        if (component is BridgeComponent<T> reactiveBridge)
        {
            return reactiveBridge.Strategy;
        }

        if (component is BridgeComponent nonReactiveBridge)
        {
            return nonReactiveBridge.Strategy;
        }

        return component;
    }

    private static bool ShouldSkip(object instance) => instance switch
    {
        ReloadableComponent => true,
        ComponentWithDisposeCallbacks => true,
        ExecutionTrackingComponent => true,
        _ => false
    };

    private static void ExpandComponents(this PipelineComponent component, List<PipelineComponent> components)
    {
        if (component is CompositeComponent pipeline)
        {
            foreach (var inner in pipeline.Components)
            {
                inner.ExpandComponents(components);
            }
        }
        else if (component is ReloadableComponent reloadable)
        {
            components.Add(reloadable);
            ExpandComponents(reloadable.Component, components);
        }
        else if (component is ExecutionTrackingComponent tracking)
        {
            components.Add(tracking);
            ExpandComponents(tracking.Component, components);
        }
        else if (component is ComponentWithDisposeCallbacks callbacks)
        {
            ExpandComponents(callbacks.Component, components);
        }
        else if (component is ExternalComponent nonDisposable)
        {
            ExpandComponents(nonDisposable.Component, components);
        }
        else
        {
            components.Add(component);
        }
    }
}
