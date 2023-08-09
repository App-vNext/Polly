namespace Polly.Utils;

internal partial class CompositeResilienceStrategy
{
    internal sealed class DebuggerProxy
    {
        private readonly CompositeResilienceStrategy _resilienceStrategy;

        public DebuggerProxy(CompositeResilienceStrategy resilienceStrategy) => _resilienceStrategy = resilienceStrategy;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<ResilienceStrategy> Strategies => _resilienceStrategy.Strategies;
    }
}
