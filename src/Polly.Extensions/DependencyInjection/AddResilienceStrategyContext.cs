using Polly.Registry;

namespace Polly.Extensions.DependencyInjection;

/// <summary>
/// Represents the context for adding a resilience strategy with the specified key.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
public sealed class AddResilienceStrategyContext<TKey>
    where TKey : notnull
{
    internal AddResilienceStrategyContext(ConfigureBuilderContext<TKey> registryContext, IServiceProvider serviceProvider)
    {
        RegistryContext = registryContext;
        ServiceProvider = serviceProvider;
        StrategyKey = registryContext.StrategyKey;
    }

    /// <summary>
    /// Gets the strategy key for the strategy being created.
    /// </summary>
    public TKey StrategyKey { get; }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> that provides access to the dependency injection container.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the context that is used by the registry.
    /// </summary>
    public ConfigureBuilderContext<TKey> RegistryContext { get; }
}
