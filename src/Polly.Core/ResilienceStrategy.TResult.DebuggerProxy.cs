namespace Polly;

public partial class ResilienceStrategy<TResult>
{
    internal class DebuggerProxy
    {
        private readonly ResilienceStrategy.DebuggerProxy _proxy;

        public DebuggerProxy(ResilienceStrategy<TResult> strategy) => _proxy = new ResilienceStrategy.DebuggerProxy(strategy.Strategy);

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<ResilienceStrategy> Strategies => _proxy.Strategies;
    }
}
