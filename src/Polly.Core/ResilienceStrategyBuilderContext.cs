using Polly.Telemetry;

namespace Polly;

#pragma warning disable S107

/// <summary>
/// The context used for building an individual resilience strategy.
/// </summary>
public sealed class ResilienceStrategyBuilderContext
{
    internal ResilienceStrategyBuilderContext(
        string? builderName,
        string? builderInstanceName,
        ResilienceProperties builderProperties,
        string? strategyName,
        TimeProvider timeProvider,
        bool isGenericBuilder,
        DiagnosticSource? diagnosticSource,
        Func<double> randomizer)
    {
        BuilderName = builderName;
        BuilderInstanceName = builderInstanceName;
        BuilderProperties = builderProperties;
        StrategyName = strategyName;
        TimeProvider = timeProvider;
        IsGenericBuilder = isGenericBuilder;
        Telemetry = TelemetryUtil.CreateTelemetry(diagnosticSource, builderName, builderInstanceName, builderProperties, strategyName);
        Randomizer = randomizer;
    }

    /// <summary>
    /// Gets the name of the builder.
    /// </summary>
    public string? BuilderName { get; }

    /// <summary>
    /// Gets the instance name of the builder.
    /// </summary>
    public string? BuilderInstanceName { get; }

    /// <summary>
    /// Gets the custom properties attached to the builder.
    /// </summary>
    public ResilienceProperties BuilderProperties { get; }

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    public string? StrategyName { get; }

    /// <summary>
    /// Gets the resilience telemetry used to report important events.
    /// </summary>
    public ResilienceStrategyTelemetry Telemetry { get; }

    /// <summary>
    /// Gets the <see cref="TimeProvider"/> used by this strategy.
    /// </summary>
    internal TimeProvider TimeProvider { get; }

    internal Func<double> Randomizer { get; }

    internal bool IsGenericBuilder { get; }
}
