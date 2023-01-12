using System.Collections.Concurrent;
using Polly;

namespace Polly.Internals;

internal class ResilienceStrategyProvider : IResilienceStrategyProvider
{
    private readonly ResilienceStrategyFactory _factory;
    private readonly ConcurrentDictionary<string, IResilienceStrategy> _pipelines = new();

    public ResilienceStrategyProvider(ResilienceStrategyFactory factory) => _factory = factory;

    public IResilienceStrategy GetResilienceStrategy(string strategyName) => _pipelines.GetOrAdd(strategyName, _ => _factory.CreateResilienceStrategy(strategyName));
}
