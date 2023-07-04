namespace Polly.Utils;

internal partial class ResilienceStrategyPipeline
{
    internal sealed class DebuggerProxy
    {
        private readonly ResilienceStrategyPipeline _resilienceStrategy;

        public DebuggerProxy(ResilienceStrategyPipeline resilienceStrategy) => _resilienceStrategy = resilienceStrategy;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<ResilienceStrategy> Strategies => _resilienceStrategy.Strategies;
    }
}
