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
        var builder = CreateBuilder();
        var component = builder.ComponentFactory();

        if (builder.ReloadTokens.Count == 0)
        {
            return (builder.Instance.ContextPool, component);
        }

        component = PipelineComponentFactory.CreateReloadable(
            new ReloadableComponent.Entry(component, builder.ReloadTokens, builder.Telemetry),
            () =>
            {
                var builder = CreateBuilder();
                return new ReloadableComponent.Entry(builder.ComponentFactory(), builder.ReloadTokens, builder.Telemetry);
            });

        return (builder.Instance.ContextPool, component);
    }

    private Builder CreateBuilder()
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

        return new(
            () =>
            {
                var innerComponent = PipelineComponentFactory.WithDisposableCallbacks(builder.BuildPipelineComponent(), context.DisposeCallbacks);
                return PipelineComponentFactory.WithExecutionTracking(innerComponent, timeProvider);
            },
            context.ReloadTokens,
            telemetry,
            builder);
    }

    private sealed record Builder(
        Func<PipelineComponent> ComponentFactory,
        List<CancellationToken> ReloadTokens,
        ResilienceStrategyTelemetry Telemetry,
        TBuilder Instance);
}
