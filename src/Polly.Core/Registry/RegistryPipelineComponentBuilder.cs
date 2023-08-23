using Polly.Telemetry;
using Polly.Utils.Pipeline;

namespace Polly.Registry;

/// <summary>
/// Builds a <see cref="PipelineComponent"/> used by the registry.
/// </summary>
internal class RegistryPipelineComponentBuilder<TBuilder, TKey>
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

    internal PipelineComponent CreateComponent()
    {
        var builder = CreateBuilder();
        var telemetry = new ResilienceStrategyTelemetry(
            new ResilienceTelemetrySource(_builderName, _instanceName, null),
            builder.Listener);

        var initialPipeline = builder.ComponentFactory();

        if (builder.ReloadTokenProducer is null)
        {
            return initialPipeline;
        }

        return PipelineComponentFactory.CreateReloadable(
            initialPipeline,
            builder.ReloadTokenProducer(),
            () => CreateBuilder().ComponentFactory(),
            telemetry);
    }

    private Builder CreateBuilder()
    {
        var context = new ConfigureBuilderContext<TKey>(_key, _builderName, _instanceName);
        var builder = _activator();
        builder.Name = _builderName;
        builder.InstanceName = _instanceName;
        _configure(builder, context);

        return new(
            builder.BuildPipelineComponent,
            context.ReloadTokenProducer,
            builder.TelemetryListener);
    }

    private record Builder(
        Func<PipelineComponent> ComponentFactory,
        Func<Func<CancellationToken>>? ReloadTokenProducer,
        TelemetryListener? Listener);
}
