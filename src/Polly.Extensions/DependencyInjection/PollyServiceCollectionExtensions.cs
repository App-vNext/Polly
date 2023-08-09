using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Polly.DependencyInjection;
using Polly.Registry;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly;

/// <summary>
/// Provides extension methods for registering resilience strategies using the <see cref="IServiceCollection"/>.
/// </summary>
public static class PollyServiceCollectionExtensions
{
    /// <summary>
    /// Adds a generic resilience strategy to service collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
    /// <typeparam name="TResult">The type of result that the resilience strategy handles.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience strategy to.</param>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="configure">An action that configures the resilience strategy.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the registered resilience strategy.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resilience strategy builder with the provided key has already been added to the registry.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// You can retrieve the registered strategy by resolving the <see cref="ResilienceStrategyProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables the telemetry for the registered resilience strategy.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddResilienceStrategy<TKey, TResult>(
        this IServiceCollection services,
        TKey key,
        Action<CompositeStrategyBuilder<TResult>> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        return services.AddResilienceStrategy<TKey, TResult>(key, (builder, _) => configure(builder));
    }

    /// <summary>
    /// Adds a generic resilience strategy to service collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
    /// <typeparam name="TResult">The type of result that the resilience strategy handles.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience strategy to.</param>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="configure">An action that configures the resilience strategy.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the registered resilience strategy.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resilience strategy builder with the provided key has already been added to the registry.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// You can retrieve the registered strategy by resolving the <see cref="ResilienceStrategyProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables the telemetry for the registered resilience strategy.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddResilienceStrategy<TKey, TResult>(
        this IServiceCollection services,
        TKey key,
        Action<CompositeStrategyBuilder<TResult>, AddResilienceStrategyContext<TKey>> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        services
            .AddOptions<ConfigureResilienceStrategyRegistryOptions<TKey>>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.Actions.Add((registry) =>
                {
                    // the last added builder with the same key wins, this allows overriding the builders
                    registry.RemoveBuilder<TResult>(key);
                    registry.TryAddBuilder<TResult>(key, (builder, context) =>
                    {
                        configure(builder, new AddResilienceStrategyContext<TKey>(context, serviceProvider));
                    });
                });
            });

        return services.AddResilienceStrategyRegistry<TKey>();
    }

    /// <summary>
    /// Adds a resilience strategy to service collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience strategy to.</param>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="configure">An action that configures the resilience strategy.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the registered resilience strategy.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resilience strategy builder with the provided key has already been added to the registry.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// You can retrieve the registered strategy by resolving the <see cref="ResilienceStrategyProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables the telemetry for the registered resilience strategy.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddResilienceStrategy<TKey>(
        this IServiceCollection services,
        TKey key,
        Action<CompositeStrategyBuilder> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        return services.AddResilienceStrategy(key, (builder, _) => configure(builder));
    }

    /// <summary>
    /// Adds a resilience strategy to service collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience strategy to.</param>
    /// <param name="key">The key used to identify the resilience strategy.</param>
    /// <param name="configure">An action that configures the resilience strategy.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the registered resilience strategy.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resilience strategy builder with the provided key has already been added to the registry.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// You can retrieve the registered strategy by resolving the <see cref="ResilienceStrategyProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables the telemetry for the registered resilience strategy.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddResilienceStrategy<TKey>(
        this IServiceCollection services,
        TKey key,
        Action<CompositeStrategyBuilder, AddResilienceStrategyContext<TKey>> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        services
            .AddOptions<ConfigureResilienceStrategyRegistryOptions<TKey>>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.Actions.Add((registry) =>
                {
                    // the last added builder with the same key wins, this allows overriding the builders
                    registry.RemoveBuilder(key);
                    registry.TryAddBuilder(key, (builder, context) =>
                    {
                        configure(builder, new AddResilienceStrategyContext<TKey>(context, serviceProvider));
                    });
                });
            });

        return services.AddResilienceStrategyRegistry<TKey>();
    }

    /// <summary>
    /// Adds <see cref="ResilienceStrategyRegistry{TKey}"/> and <see cref="ResilienceStrategyProvider{TKey}"/> that allows configuring
    /// and retrieving resilience strategies using the <typeparamref name="TKey"/> key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience strategy to.</param>
    /// <param name="configure">The action that configures the <see cref="ResilienceStrategyRegistryOptions{TKey}"/> that are used by the registry.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with additional services added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This call enables telemetry for all resilience strategies created using <see cref="ResilienceStrategyRegistry{TKey}"/>.
    /// </remarks>
    public static IServiceCollection AddResilienceStrategyRegistry<TKey>(
        this IServiceCollection services,
        Action<ResilienceStrategyRegistryOptions<TKey>> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        services.AddResilienceStrategyRegistry<TKey>();
        services.Configure(configure);

        return services;
    }

    /// <summary>
    /// Adds <see cref="ResilienceStrategyRegistry{TKey}"/> and <see cref="ResilienceStrategyProvider{TKey}"/> that allows configuring
    /// and retrieving resilience strategies using the <typeparamref name="TKey"/> key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience strategy.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience strategy to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with additional services added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This call enables telemetry for all resilience strategies created using <see cref="ResilienceStrategyRegistry{TKey}"/>.
    /// </remarks>
    public static IServiceCollection AddResilienceStrategyRegistry<TKey>(this IServiceCollection services)
        where TKey : notnull
    {
        Guard.NotNull(services);

        // check marker to ensure the APIs bellow are called only once for each TKey type
        // this prevents polluting the service collection with unnecessary Configure calls
        if (services.Contains(RegistryMarker<TKey>.ServiceDescriptor))
        {
            return services;
        }

        services.AddOptions();
        services.Add(RegistryMarker<TKey>.ServiceDescriptor);
        services.AddCompositeStrategyBuilder();

        services.TryAddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ResilienceStrategyRegistryOptions<TKey>>>().Value;
            var configureActions = serviceProvider.GetRequiredService<IOptions<ConfigureResilienceStrategyRegistryOptions<TKey>>>().Value.Actions;
            var registry = new ResilienceStrategyRegistry<TKey>(options);

            foreach (var entry in configureActions)
            {
                entry(registry);
            }

            return registry;
        });

        services.TryAddSingleton<ResilienceStrategyProvider<TKey>>(serviceProvider => serviceProvider.GetRequiredService<ResilienceStrategyRegistry<TKey>>());

        // configure options
        services
            .AddOptions<ResilienceStrategyRegistryOptions<TKey>>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.BuilderFactory = () => serviceProvider.GetRequiredService<CompositeStrategyBuilder>();
            });

        return services;
    }

    private static void AddCompositeStrategyBuilder(this IServiceCollection services)
    {
        services
            .AddOptions<TelemetryOptions>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.LoggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            });

        services.TryAddTransient(serviceProvider =>
        {
            var builder = new CompositeStrategyBuilder();
            builder.Properties.Set(PollyDependencyInjectionKeys.ServiceProvider, serviceProvider);
            builder.ConfigureTelemetry(serviceProvider.GetRequiredService<IOptions<TelemetryOptions>>().Value);
            return builder;
        });
    }

    private class RegistryMarker<TKey>
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton(new RegistryMarker<TKey>());
    }
}
