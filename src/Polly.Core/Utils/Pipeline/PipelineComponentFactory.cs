using Polly.Telemetry;

namespace Polly.Utils.Pipeline;

internal static class PipelineComponentFactory
{
    public static PipelineComponent FromPipeline(ResiliencePipeline pipeline) => new ExternalComponent(pipeline.Component);

    public static PipelineComponent FromPipeline<T>(ResiliencePipeline<T> pipeline) => new ExternalComponent(pipeline.Component);

    public static PipelineComponent FromStrategy(ResilienceStrategy strategy) => new BridgeComponent(strategy);

    public static PipelineComponent FromStrategy<T>(ResilienceStrategy<T> strategy) => new BridgeComponent<T>(strategy);

    public static PipelineComponent WithDisposableCallbacks(PipelineComponent component, List<Action> callbacks)
        => callbacks.Count == 0 ? component : new ComponentWithDisposeCallbacks(component, callbacks);

    public static PipelineComponent WithExecutionTracking(PipelineComponent component, TimeProvider timeProvider) => new ExecutionTrackingComponent(component, timeProvider);

    public static PipelineComponent CreateComposite(
        IReadOnlyList<PipelineComponent> components,
        ResilienceStrategyTelemetry telemetry,
        TimeProvider timeProvider) => CompositeComponent.Create(components, telemetry, timeProvider);

    public static PipelineComponent CreateReloadable(
        ReloadableComponent.Entry initial,
        Func<ReloadableComponent.Entry> factory) => new ReloadableComponent(initial, factory);
}
