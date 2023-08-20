namespace Polly.Utils.Pipeline;

internal sealed class CompositeComponentDebuggerProxy
{
    private readonly CompositeComponent _pipeline;

    public CompositeComponentDebuggerProxy(CompositeComponent pipeline) => _pipeline = pipeline;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IEnumerable<PipelineComponent> Strategies => _pipeline.Components;
}
