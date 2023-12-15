namespace Polly.DependencyInjection;

/// <summary>
/// Represents the context for configuring resilience pipelines with the specified key.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the resilience pipeline.</typeparam>
public class ConfigureResiliencePipelineRegistryContext<TKey>
    where TKey : notnull
{
    private readonly ConfigureResiliencePipelineRegistryOptions<TKey> _options;
    private readonly IServiceProvider _serviceProvider;

    internal ConfigureResiliencePipelineRegistryContext(
        ConfigureResiliencePipelineRegistryOptions<TKey> options,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Adds a resilience pipeline to the registry.
    /// </summary>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="configure">An action that configures the resilience pipeline.</param>
    /// <returns>The current <see cref="ConfigureResiliencePipelineRegistryContext{TKey}"/> to allow calls to be chained.</returns>
    /// <remarks>
    /// You can retrieve the registered pipeline by resolving the <see cref="Registry.ResiliencePipelineProvider{TKey}"/> class from the dependency injection container.
    /// <para>
    /// This call enables the telemetry for the registered resilience pipeline.
    /// </para>
    /// </remarks>
    public ConfigureResiliencePipelineRegistryContext<TKey> AddResiliencePipeline(TKey key,
        Action<ResiliencePipelineBuilder, AddResiliencePipelineContext<TKey>> configure)
    {
        _options.Actions.Add((registry) =>
        {
            // the last added builder with the same key wins, this allows overriding the builders
            registry.TryAddBuilder(key, (builder, context) =>
            {
                configure(builder, new AddResiliencePipelineContext<TKey>(context, _serviceProvider));
            });
        });
        return this;
    }
}
