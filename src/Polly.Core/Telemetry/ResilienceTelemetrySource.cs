namespace Polly.Telemetry;

/// <summary>
/// The source of resilience telemetry events.
/// </summary>
/// <remarks>
/// This class is used by the telemetry infrastructure and should not be used directly by user code.
/// </remarks>
public sealed class ResilienceTelemetrySource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceTelemetrySource"/> class.
    /// </summary>
    /// <param name="builderName">The builder name.</param>
    /// <param name="builderInstanceName">The builder instance name.</param>
    /// <param name="strategyName">The strategy name.</param>
    public ResilienceTelemetrySource(
        string? builderName,
        string? builderInstanceName,
        string? strategyName)
    {
        BuilderName = builderName;
        BuilderInstanceName = builderInstanceName;
        StrategyName = strategyName;
    }

    /// <summary>
    /// Gets the builder name.
    /// </summary>
    public string? BuilderName { get; }

    /// <summary>
    /// Gets the builder instance name.
    /// </summary>
    public string? BuilderInstanceName { get; }

    /// <summary>
    /// Gets the strategy name.
    /// </summary>
    public string? StrategyName { get; }
}

