namespace Polly;

public abstract partial class ResilienceStrategy
{
    internal class DebuggerProxy
    {
        private readonly ResilienceStrategy _resilienceStrategy;

        public DebuggerProxy(ResilienceStrategy resilienceStrategy) => _resilienceStrategy = resilienceStrategy;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<ResilienceStrategy> Strategies => UnwrapStrategies(_resilienceStrategy);

        private IEnumerable<ResilienceStrategy> UnwrapStrategies(ResilienceStrategy strategy)
        {
            if (strategy is ResilienceStrategyPipeline pipeline)
            {
                return pipeline.Strategies;
            }

            if (strategy is ReloadableResilienceStrategy reloadableResilienceStrategy)
            {
                var list = new List<ResilienceStrategy>
                {
                    strategy
                };
                list.AddRange(UnwrapStrategies(reloadableResilienceStrategy.Strategy));

                return list;
            }

            return new[] { strategy };
        }
    }
}
