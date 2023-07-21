namespace Polly.Telemetry;

/// <summary>
/// The source of resilience telemetry events.
/// </summary>
/// <param name="BuilderName">The builder name.</param>
/// <param name="BuilderInstanceName">The builder instance name.</param>
/// <param name="BuilderProperties">The builder properties.</param>
/// <param name="StrategyName">The strategy name. </param>
/// <remarks>
/// This class is used by the telemetry infrastructure and should not be used directly by user code.
/// </remarks>
public sealed record class ResilienceTelemetrySource(
    string? BuilderName,
    string? BuilderInstanceName,
    ResilienceProperties BuilderProperties,
    string? StrategyName);

