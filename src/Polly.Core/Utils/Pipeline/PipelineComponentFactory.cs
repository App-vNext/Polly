using Polly.Telemetry;

namespace Polly.Utils.Pipeline;

internal static class PipelineComponentFactory
{
    public static PipelineComponent FromPipeline(ResiliencePipeline pipeline) => new ExternalComponent(pipeline.Component);

    public static PipelineComponent FromPipeline<T>(ResiliencePipeline<T> pipeline) => new ExternalComponent(pipeline.Component);

    public static PipelineComponent FromStrategy(ResilienceStrategy strategy) => new BridgeComponent(strategy);

    public static PipelineComponent FromStrategy<T>(ResilienceStrategy<T> strategy) => new BridgeComponent<T>(strategy);

    public static PipelineComponent WithDisposableCallbacks(PipelineComponent component, IEnumerable<Action> callbacks)
    {
#pragma warning disable CA1851 // Possible multiple enumerations of 'IEnumerable' collection
#if NET6_0_OR_GREATER
        if (callbacks.TryGetNonEnumeratedCount(out var count))
        {
            if (count == 0)
            {
                return component;
            }
        }
        else if (!callbacks.Any())
#else
        if (!callbacks.Any())
#endif
        {
            return component;
        }

        return new ComponentWithDisposeCallbacks(component, callbacks.ToList());
#pragma warning restore CA1851 // Possible multiple enumerations of 'IEnumerable' collection
    }

    public static PipelineComponent WithExecutionTracking(PipelineComponent component, TimeProvider timeProvider) => new ExecutionTrackingComponent(component, timeProvider);

    public static PipelineComponent CreateComposite(
        IReadOnlyList<PipelineComponent> components,
        ResilienceStrategyTelemetry telemetry,
        TimeProvider timeProvider) => CompositeComponent.Create(components, telemetry, timeProvider);

    public static PipelineComponent CreateReloadable(
        ReloadableComponent.Entry initial,
        Func<ReloadableComponent.Entry> factory) => new ReloadableComponent(initial, factory);
}
