using Polly.Strategy;

namespace Polly.Telemetry;

internal static class TelemetryUtil
{
    internal const string PollyDiagnosticSource = "Polly";

    internal static readonly ResiliencePropertyKey<DiagnosticSource> DiagnosticSourceKey = new("DiagnosticSource");

    internal static readonly ResiliencePropertyKey<string> StrategyKey = new("StrategyKey");

    public static ResilienceStrategyTelemetry CreateTelemetry(string? builderName, ResilienceProperties builderProperties, string? strategyName, string strategyType)
    {
        builderProperties.TryGetValue(DiagnosticSourceKey, out var diagnosticSource);

        var telemetrySource = new ResilienceTelemetrySource(builderName, builderProperties, strategyName, strategyType);

        return new ResilienceStrategyTelemetry(telemetrySource, diagnosticSource);
    }
}
