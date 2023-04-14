namespace Polly.Telemetry;

internal sealed record class ResilienceTelemetrySource(string BuilderName, ResilienceProperties BuilderProperties, string StrategyName, string StrategyType);

