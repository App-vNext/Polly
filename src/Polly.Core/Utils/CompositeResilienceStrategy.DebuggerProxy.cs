namespace Polly.Utils;

internal partial class CompositeResilienceStrategy<T>
{
    internal sealed class DebuggerProxy
    {
        private readonly CompositeResilienceStrategy<T> _resilienceStrategy;

        public DebuggerProxy(CompositeResilienceStrategy<T> resilienceStrategy) => _resilienceStrategy = resilienceStrategy;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<ResilienceStrategy<T>> Strategies => _resilienceStrategy.Strategies;
    }
}
