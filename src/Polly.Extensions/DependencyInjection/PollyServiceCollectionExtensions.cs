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
/// Provides extension methods for registering resilience pipelines using the <see cref="IServiceCollection"/>.
/// </summary>
public static class PollyServiceCollectionExtensions
{
    /// <summary>
    /// Adds a generic resilience pipeline to service collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience pipeline.</typeparam>
    /// <typeparam name="TResult">The type of result that the resilience pipeline handles.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience pipeline to.</param>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">An action that configures the resilience pipeline.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the registered resilience pipeline.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resilience pipeline builder with the provided key has already been added to the registry.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// You can retrieve the registered pipeline by resolving the <see cref="ResiliencePipelineProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables the telemetry for the registered resilience pipeline.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddResiliencePipeline<TKey, TResult>(
        this IServiceCollection services,
        TKey key,
        Action<ResiliencePipelineBuilder<TResult>> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        return services.AddResiliencePipeline<TKey, TResult>(key, (builder, _) => configure(builder));
    }

    /// <summary>
    /// Adds a generic resilience pipeline to service collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience pipeline.</typeparam>
    /// <typeparam name="TResult">The type of result that the resilience pipeline handles.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience pipeline to.</param>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">An action that configures the resilience pipeline.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the registered resilience pipeline.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resilience pipeline builder with the provided key has already been added to the registry.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// You can retrieve the registered pipeline by resolving the <see cref="ResiliencePipelineProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables the telemetry for the registered resilience pipeline.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddResiliencePipeline<TKey, TResult>(
        this IServiceCollection services,
        TKey key,
        Action<ResiliencePipelineBuilder<TResult>, AddResiliencePipelineContext<TKey>> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        services
            .AddOptions<ConfigureResiliencePipelineRegistryOptions<TKey>>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.Actions.Add((registry) =>
                {
                    // the last added builder with the same key wins, this allows overriding the builders
                    registry.RemoveBuilder<TResult>(key);
                    registry.TryAddBuilder<TResult>(key, (builder, context) =>
                    {
                        configure(builder, new AddResiliencePipelineContext<TKey>(context, serviceProvider));
                    });
                });
            });

        return services.AddResiliencePipelineRegistry<TKey>();
    }

    /// <summary>
    /// Adds a resilience pipeline to service collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience pipeline.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience pipeline to.</param>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">An action that configures the resilience pipeline.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the registered resilience pipeline.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resilience pipeline builder with the provided key has already been added to the registry.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// You can retrieve the registered pipeline by resolving the <see cref="ResiliencePipelineProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables the telemetry for the registered resilience pipeline.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddResiliencePipeline<TKey>(
        this IServiceCollection services,
        TKey key,
        Action<ResiliencePipelineBuilder> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        return services.AddResiliencePipeline(key, (builder, _) => configure(builder));
    }

    /// <summary>
    /// Adds a resilience pipeline to service collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience pipeline.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience pipeline to.</param>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">An action that configures the resilience pipeline.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the registered resilience pipeline.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resilience pipeline builder with the provided key has already been added to the registry.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// You can retrieve the registered pipeline by resolving the <see cref="ResiliencePipelineProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables the telemetry for the registered resilience pipeline.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddResiliencePipeline<TKey>(
        this IServiceCollection services,
        TKey key,
        Action<ResiliencePipelineBuilder, AddResiliencePipelineContext<TKey>> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        services
            .AddOptions<ConfigureResiliencePipelineRegistryOptions<TKey>>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.Actions.Add((registry) =>
                {
                    // the last added builder with the same key wins, this allows overriding the builders
                    registry.RemoveBuilder(key);
                    registry.TryAddBuilder(key, (builder, context) =>
                    {
                        configure(builder, new AddResiliencePipelineContext<TKey>(context, serviceProvider));
                    });
                });
            });

        return services.AddResiliencePipelineRegistry<TKey>();
    }

    /// <summary>
    /// Adds <see cref="ResiliencePipelineRegistry{TKey}"/> and <see cref="ResiliencePipelineProvider{TKey}"/> that allows configuring
    /// and retrieving resilience pipelines using the <typeparamref name="TKey"/> key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience pipeline.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience pipeline to.</param>
    /// <param name="configure">The action that configures the <see cref="ResiliencePipelineRegistryOptions{TKey}"/> that are used by the registry.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with additional services added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This call enables telemetry for all resilience pipelines created using <see cref="ResiliencePipelineRegistry{TKey}"/>.
    /// </remarks>
    public static IServiceCollection AddResiliencePipelineRegistry<TKey>(
        this IServiceCollection services,
        Action<ResiliencePipelineRegistryOptions<TKey>> configure)
        where TKey : notnull
    {
        Guard.NotNull(services);
        Guard.NotNull(configure);

        services.AddResiliencePipelineRegistry<TKey>();
        services.Configure(configure);

        return services;
    }

    /// <summary>
    /// Adds <see cref="ResiliencePipelineRegistry{TKey}"/> and <see cref="ResiliencePipelineProvider{TKey}"/> that allows configuring
    /// and retrieving resilience pipelines using the <typeparamref name="TKey"/> key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the resilience pipeline.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the resilience pipeline to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with additional services added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This call enables telemetry for all resilience pipelines created using <see cref="ResiliencePipelineRegistry{TKey}"/>.
    /// </remarks>
    public static IServiceCollection AddResiliencePipelineRegistry<TKey>(this IServiceCollection services)
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
        services.AddResiliencePipelineBuilder();

        services.TryAddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ResiliencePipelineRegistryOptions<TKey>>>().Value;
            var configureActions = serviceProvider.GetRequiredService<IOptions<ConfigureResiliencePipelineRegistryOptions<TKey>>>().Value.Actions;
            var registry = new ResiliencePipelineRegistry<TKey>(options);

            foreach (var entry in configureActions)
            {
                entry(registry);
            }

            return registry;
        });

        services.TryAddSingleton<ResiliencePipelineProvider<TKey>>(serviceProvider => serviceProvider.GetRequiredService<ResiliencePipelineRegistry<TKey>>());

        // configure options
        services
            .AddOptions<ResiliencePipelineRegistryOptions<TKey>>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.BuilderFactory = () => serviceProvider.GetRequiredService<ResiliencePipelineBuilder>();
            });

        return services;
    }

    private static void AddResiliencePipelineBuilder(this IServiceCollection services)
    {
        services
            .AddOptions<TelemetryOptions>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.LoggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            });

        services.TryAddTransient(serviceProvider =>
        {
            var builder = new ResiliencePipelineBuilder();
            builder.ConfigureTelemetry(serviceProvider.GetRequiredService<IOptions<TelemetryOptions>>().Value);
            return builder;
        });
    }

    private class RegistryMarker<TKey>
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton(new RegistryMarker<TKey>());
    }
}
