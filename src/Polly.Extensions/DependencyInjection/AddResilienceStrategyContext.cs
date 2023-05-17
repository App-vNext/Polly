namespace Polly.Extensions.DependencyInjection;

/// <summary>
/// Represents the context for adding a resilience strategy with the specified key.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
public sealed class AddResilienceStrategyContext<TKey>
    where TKey : notnull
{
    internal AddResilienceStrategyContext(TKey key, IServiceProvider serviceProvider)
    {
        Key = key;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the key used to identify the resilience strategy.
    /// </summary>
    public TKey Key { get; }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> that provides access to the dependency injection container.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }
}
