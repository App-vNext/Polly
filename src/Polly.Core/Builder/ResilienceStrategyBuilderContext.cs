using Polly.Telemetry;

namespace Polly.Builder;

/// <summary>
/// The context used for building an individual resilience strategy.
/// </summary>
public class ResilienceStrategyBuilderContext
{
    /// <summary>
    /// Gets the name of the builder.
    /// </summary>
    public string BuilderName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the custom properties attached to the builder.
    /// </summary>
    public ResilienceProperties BuilderProperties { get; internal set; } = new();

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    public string StrategyName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the type of the strategy.
    /// </summary>
    public string StrategyType { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the resilience telemetry used to report important events.
    /// </summary>
    public ResilienceTelemetry Telemetry { get; internal set; } = NullResilienceTelemetry.Instance;

    /// <summary>
    /// Gets or sets the <see cref="TimeProvider"/> used by this strategy.
    /// </summary>
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;
}
