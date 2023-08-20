using Polly.Registry;

namespace Polly.DependencyInjection;

internal sealed class ConfigureResilienceStrategyRegistryOptions<TKey>
    where TKey : notnull
{
    public List<Action<ResilienceStrategyRegistry<TKey>>> Actions { get; } = new();
}
