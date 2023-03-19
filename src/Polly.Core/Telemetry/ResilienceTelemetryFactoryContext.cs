namespace Polly.Telemetry;

/// <summary>
/// The context used for building an instance of <see cref="ResilienceTelemetry"/>.
/// </summary>
public class ResilienceTelemetryFactoryContext
{
    /// <summary>
    /// Gets the name of the builder.
    /// </summary>
    public string BuilderName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    public string StrategyName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the type of the strategy.
    /// </summary>
    public string StrategyType { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the custom properties attached to the builder.
    /// </summary>
    public ResilienceProperties BuilderProperties { get; internal set; } = new();
}
