using Polly.Telemetry;

namespace Polly;

/// <summary>
/// The context used for building an individual resilience strategy.
/// </summary>
public sealed class StrategyBuilderContext
{
    internal StrategyBuilderContext(
        string? builderName,
        string? builderInstanceName,
        string? strategyName,
        TimeProvider timeProvider,
        TelemetryListener? telemetryListener)
    {
        BuilderName = builderName;
        BuilderInstanceName = builderInstanceName;
        StrategyName = strategyName;
        TimeProvider = timeProvider;
        Telemetry = TelemetryUtil.CreateTelemetry(telemetryListener, builderName, builderInstanceName, strategyName);
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
}
