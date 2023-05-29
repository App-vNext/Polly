using Polly.Telemetry;

namespace Polly.Strategy;

/// <summary>
/// The context used for building an individual resilience strategy.
/// </summary>
public sealed class ResilienceStrategyBuilderContext
{
    internal ResilienceStrategyBuilderContext(
        string builderName,
        ResilienceProperties builderProperties,
        string strategyName,
        string strategyType,
        TimeProvider timeProvider)
    {
        BuilderName = builderName;
        BuilderProperties = builderProperties;
        StrategyName = strategyName;
        StrategyType = strategyType;
        TimeProvider = timeProvider;
        Telemetry = TelemetryUtil.CreateTelemetry(builderName, builderProperties, strategyName, strategyType);
    }

    /// <summary>
    /// Gets the name of the builder.
    /// </summary>
    public string BuilderName { get; }

    /// <summary>
    /// Gets the custom properties attached to the builder.
    /// </summary>
    public ResilienceProperties BuilderProperties { get; }

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    public string StrategyName { get; }

    /// <summary>
    /// Gets the type of the strategy.
    /// </summary>
    public string StrategyType { get; }

    /// <summary>
    /// Gets the resilience telemetry used to report important events.
    /// </summary>
    public ResilienceStrategyTelemetry Telemetry { get; }

    /// <summary>
    /// Gets the <see cref="TimeProvider"/> used by this strategy.
    /// </summary>
    internal TimeProvider TimeProvider { get; }
}
