using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Polly.Builder;
using Polly.Registry;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering resilience strategies using the <see cref="IServiceCollection"/>.
/// </summary>
public static class PollyServiceCollectionExtensions
{
    /// <summary>
    /// Adds a resilience strategy to service collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience strategy to.</param>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="configure">An action that configures the resilience strategy.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the registered resilience strategy.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resilience strategy builder with the provided key has already been added to the registry.</exception>
    /// <remarks>
    /// You can retrieve the registered strategy by resolving the <see cref="ResilienceStrategyProvider{TKey}"/> class from the dependency injection container.
    /// </remarks>
    public static IServiceCollection AddResilienceStrategy<TKey>(
        this IServiceCollection services,
        TKey key,
        Action<AddResilienceStrategyContext<TKey>> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        services.AddOptions();
        services.Configure<ConfigureResilienceStrategyRegistryOptions<TKey>>(options =>
        {
            options.Actions.Add(new ConfigureResilienceStrategyRegistryOptions<TKey>.Entry(key, configure));
        });

        // check marker to ensure the APIs bellow are called only once for each TKey type
        // this prevents polluting the service collection with unnecessary Configure calls
        if (services.Contains(RegistryMarker<TKey>.ServiceDescriptor))
        {
            return services;
        }

        services.Add(RegistryMarker<TKey>.ServiceDescriptor);
        services.AddResilienceStrategyBuilder();
        services.AddResilienceStrategyRegistry<TKey>();

        return services;
    }

    private static IServiceCollection AddResilienceStrategyRegistry<TKey>(this IServiceCollection services)
        where TKey : notnull
    {
        services.TryAddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ResilienceStrategyRegistryOptions<TKey>>>().Value;
            var configureActions = serviceProvider.GetRequiredService<IOptions<ConfigureResilienceStrategyRegistryOptions<TKey>>>().Value.Actions;
            var registry = new ResilienceStrategyRegistry<TKey>(options);

            foreach (var entry in configureActions)
            {
                // the last added builder with the same key wins, this allows overriding the builders
                registry.RemoveBuilder(entry.Key);
                registry.TryAddBuilder(entry.Key, (key, builder) =>
                {
                    var context = new AddResilienceStrategyContext<TKey>(key, builder, serviceProvider);
                    entry.Configure(context);
                });
            }

            return registry;
        });

        services.TryAddSingleton<ResilienceStrategyProvider<TKey>>(serviceProvider =>
        {
            return serviceProvider.GetRequiredService<ResilienceStrategyRegistry<TKey>>();
        });

        // configure options
        services
            .AddOptions<ResilienceStrategyRegistryOptions<TKey>>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.BuilderFactory = () => serviceProvider.GetRequiredService<ResilienceStrategyBuilder>();
            });

        return services;
    }

    private static void AddResilienceStrategyBuilder(this IServiceCollection services)
    {
        services.TryAddTransient(serviceProvider =>
        {
            var builder = new ResilienceStrategyBuilder();
            if (serviceProvider.GetService<ResilienceTelemetryFactory>() is ResilienceTelemetryFactory factory)
            {
                builder.TelemetryFactory = factory;
            }

            builder.Properties.Set(PollyDependencyInjectionKeys.ServiceProvider, serviceProvider);
            return builder;
        });
    }

    private class RegistryMarker<TKey>
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton(new RegistryMarker<TKey>());
    }
}
