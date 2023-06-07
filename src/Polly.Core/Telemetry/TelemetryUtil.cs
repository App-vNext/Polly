namespace Polly.Telemetry;

internal static class TelemetryUtil
{
    internal const string PollyDiagnosticSource = "Polly";

    internal static readonly ResiliencePropertyKey<string> StrategyKey = new("Polly.StrategyKey");

    public static ResilienceStrategyTelemetry CreateTelemetry(
        DiagnosticSource? diagnosticSource,
        string? builderName,
        ResilienceProperties builderProperties,
        string? strategyName,
        string strategyType)
    {
        var telemetrySource = new ResilienceTelemetrySource(builderName, builderProperties, strategyName, strategyType);

        return new ResilienceStrategyTelemetry(telemetrySource, diagnosticSource);
    }
}
