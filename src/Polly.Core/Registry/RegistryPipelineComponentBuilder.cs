using Polly.Telemetry;
using Polly.Utils.Pipeline;

namespace Polly.Registry;

/// <summary>
/// Builds a <see cref="PipelineComponent"/> used by the registry.
/// </summary>
internal sealed class RegistryPipelineComponentBuilder<TBuilder, TKey>
    where TBuilder : ResiliencePipelineBuilderBase
    where TKey : notnull
{
    private readonly Func<TBuilder> _activator;
    private readonly TKey _key;
    private readonly string _builderName;
    private readonly string? _instanceName;
    private readonly Action<TBuilder, ConfigureBuilderContext<TKey>> _configure;

    public RegistryPipelineComponentBuilder(
        Func<TBuilder> activator,
        TKey key,
        string builderName,
        string? instanceName,
        Action<TBuilder, ConfigureBuilderContext<TKey>> configure)
    {
        _activator = activator;
        _key = key;
        _builderName = builderName;
        _instanceName = instanceName;
        _configure = configure;
    }

    internal (ResilienceContextPool? ContextPool, PipelineComponent Component) CreateComponent()
    {
        var (component, reloadTokens, telemetry, instance) = CreateBuilder();

        if (reloadTokens.Count == 0)
        {
            return (instance.ContextPool, component);
        }

        component = PipelineComponentFactory.CreateReloadable(
            new ReloadableComponent.Entry(component, reloadTokens, telemetry),
            () =>
            {
                var (component, reloadTokens, telemetry, _) = CreateBuilder();
                return new ReloadableComponent.Entry(component, reloadTokens, telemetry);
            });

        return (instance.ContextPool, component);
    }

    private (PipelineComponent Component, List<CancellationToken> ReloadTokens, ResilienceStrategyTelemetry Telemetry, TBuilder Instance) CreateBuilder()
    {
        var context = new ConfigureBuilderContext<TKey>(_key, _builderName, _instanceName);
        var builder = _activator();
        builder.Name = _builderName;
        builder.InstanceName = _instanceName;
        _configure(builder, context);

        var timeProvider = builder.TimeProviderInternal;
        var telemetry = new ResilienceStrategyTelemetry(
            new ResilienceTelemetrySource(builder.Name, builder.InstanceName, null),
            builder.TelemetryListener);

        var innerComponent = PipelineComponentFactory.WithDisposableCallbacks(builder.BuildPipelineComponent(), context.DisposeCallbacks);
        var component = PipelineComponentFactory.WithExecutionTracking(innerComponent, timeProvider);

        return new(component, context.ReloadTokens, telemetry, builder);
    }
}
