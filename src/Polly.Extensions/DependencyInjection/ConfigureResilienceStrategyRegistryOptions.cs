namespace Polly.Extensions.DependencyInjection;

internal sealed class ConfigureResilienceStrategyRegistryOptions<TKey>
    where TKey : notnull
{
    public List<Entry> Actions { get; } = new();

    public record Entry(TKey Key, Action<AddResilienceStrategyContext<TKey>> Configure);
}
