namespace Polly.Utils;

internal partial class CompositeResiliencePipeline
{
    internal sealed class DebuggerProxy
    {
        private readonly CompositeResiliencePipeline _pipeline;

        public DebuggerProxy(CompositeResiliencePipeline pipeline) => _pipeline = pipeline;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<ResiliencePipeline> Strategies => _pipeline.Strategies;
    }
}
