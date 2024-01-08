using Polly.Utils;

namespace Polly.DependencyInjection;

/// <summary>
/// Represents the context for configuring resilience pipelines with the specified key.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the resilience pipeline.</typeparam>
public sealed class AddResiliencePipelinesContext<TKey>
    where TKey : notnull
{
    private readonly ConfigureResiliencePipelineRegistryOptions<TKey> _options;

    internal AddResiliencePipelinesContext(
        ConfigureResiliencePipelineRegistryOptions<TKey> options,
        IServiceProvider serviceProvider)
    {
        _options = options;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> that provides access to the dependency injection container.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Adds a resilience pipeline to the registry.
    /// </summary>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">An action that configures the resilience pipeline.</param>
    /// <remarks>
    /// You can retrieve the registered pipeline by resolving the <see cref="Registry.ResiliencePipelineProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables telemetry for the registered resilience pipeline.
    /// </para>
    /// </remarks>
    public void AddResiliencePipeline(
        TKey key,
        Action<ResiliencePipelineBuilder, AddResiliencePipelineContext<TKey>> configure)
    {
        Guard.NotNull(configure);

        _options.Actions.Add((registry) =>
        {
            // the last added builder with the same key wins, this allows overriding the builders
            registry.TryAddBuilder(key, (builder, context) =>
            {
                configure(builder, new AddResiliencePipelineContext<TKey>(context, ServiceProvider));
            });
        });
    }

    /// <summary>
    /// Adds a resilience pipeline to the registry.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience pipeline handles.</typeparam>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">An action that configures the resilience pipeline.</param>
    /// <remarks>
    /// You can retrieve the registered pipeline by resolving the <see cref="Registry.ResiliencePipelineProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables telemetry for the registered resilience pipeline.
    /// </para>
    /// </remarks>
    public void AddResiliencePipeline<TResult>(
        TKey key,
        Action<ResiliencePipelineBuilder<TResult>, AddResiliencePipelineContext<TKey>> configure)
    {
        Guard.NotNull(configure);

        _options.Actions.Add((registry) =>
        {
            // the last added builder with the same key wins, this allows overriding the builders
            registry.TryAddBuilder<TResult>(key, (builder, context) =>
            {
                configure(builder, new AddResiliencePipelineContext<TKey>(context, ServiceProvider));
            });
        });
    }
}
