using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly.Telemetry;

namespace Polly.Utils;

/// <summary>
/// Represents a single component of a resilience pipeline.
/// </summary>
/// <remarks>
/// The component of the pipeline can be either a strategy, a generic strategy or a whole pipeline.
/// </remarks>
internal abstract partial class PipelineComponent : IDisposable, IAsyncDisposable
{
    public static PipelineComponent Null { get; } = new NullComponent();

    internal ResilienceStrategyOptions? Options { get; set; }

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

    internal abstract ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state);

    public abstract void Dispose();

    public abstract ValueTask DisposeAsync();

    private class NullComponent : PipelineComponent
    {
        internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => callback(context, state);

        public override void Dispose()
        {
        }

        public override ValueTask DisposeAsync() => default;
    }
}
