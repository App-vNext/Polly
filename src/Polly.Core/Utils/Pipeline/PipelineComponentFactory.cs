using System;
using System.Collections.Generic;
using Polly.Telemetry;
namespace Polly.Utils.Pipeline;

internal static class PipelineComponentFactory
{
    public static PipelineComponent FromPipeline(ResiliencePipeline pipeline) => pipeline.Component;

    public static PipelineComponent FromPipeline<T>(ResiliencePipeline<T> pipeline) => pipeline.Component;

    public static PipelineComponent FromStrategy(ResilienceStrategy strategy) => new BridgeComponent(strategy);

    public static PipelineComponent FromStrategy<T>(ResilienceStrategy<T> strategy) => new BridgeComponent<T>(strategy);

    public static PipelineComponent CreateComposite(
        IReadOnlyList<PipelineComponent> components,
        ResilienceStrategyTelemetry telemetry,
        TimeProvider timeProvider) => CompositeComponent.Create(components, telemetry, timeProvider);

    public static PipelineComponent CreateReloadable(
        PipelineComponent initialComponent,
        Func<CancellationToken> onReload,
        Func<PipelineComponent> factory,
        ResilienceStrategyTelemetry telemetry) => new ReloadableComponent(initialComponent, onReload, factory, telemetry);
}
