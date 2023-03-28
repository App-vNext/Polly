namespace Polly.DependencyInjection;

internal class ConfigureResilienceStrategyRegistryOptions<TKey>
    where TKey : notnull
{
    public List<Entry> Actions { get; } = new();

    public record Entry(TKey Key, Action<AddResilienceStrategyContext<TKey>> Configure);
}
