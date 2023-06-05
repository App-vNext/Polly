using System.ComponentModel;

namespace Polly.Telemetry;

/// <summary>
/// The source of resilience telemetry events.
/// </summary>
/// <param name="BuilderName">The builder name.</param>
/// <param name="BuilderProperties">The builder properties.</param>
/// <param name="StrategyName">The strategy name. </param>
/// <param name="StrategyType">The strategy type.</param>
/// <remarks>
/// This class is used by the telemetry infrastructure and should not be used directly by user code.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed record class ResilienceTelemetrySource(
    string BuilderName,
    ResilienceProperties BuilderProperties,
    string StrategyName,
    string StrategyType);

