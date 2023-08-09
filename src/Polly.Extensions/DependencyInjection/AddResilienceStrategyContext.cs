using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Polly.DependencyInjection;

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
        BuilderName = registryContext.BuilderName;
    }

    /// <summary>
    /// Gets the strategy key for the strategy being created.
    /// </summary>
    public string BuilderName { get; }

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
    internal ConfigureBuilderContext<TKey> RegistryContext { get; }

    /// <summary>
    /// Enables dynamic reloading of the resilience strategy whenever the <typeparamref name="TOptions"/> options are changed.
    /// </summary>
    /// <typeparam name="TOptions">The options type to listen to.</typeparam>
    /// <param name="name">The named options, if any.</param>
    /// <remarks>
    /// You can decide based on the <paramref name="name"/> to listen for changes in global options or named options.
    /// If <paramref name="name"/> is <see langword="null"/> then the global options are listened to.
    /// <para>
    /// You can listen for changes only for single options. If you call this method multiple times, the preceding calls are ignored and only the last one wins.
    /// </para>
    /// </remarks>
    public void EnableReloads<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(string? name = null)
        => RegistryContext.EnableReloads(ServiceProvider.GetRequiredService<IOptionsMonitor<TOptions>>(), name);

    /// <summary>
    /// Gets the options identified by <paramref name="name"/>.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <param name="name">The options name, if any.</param>
    /// <returns>The options instance.</returns>
    /// <remarks>
    /// If <paramref name="name"/> is <see langword="null"/> then the global options are returned.
    /// </remarks>
    public TOptions GetOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions>(string? name = null)
    {
        var monitor = ServiceProvider.GetRequiredService<IOptionsMonitor<TOptions>>();

        return name == null ? monitor.CurrentValue : monitor.Get(name);
    }
}
