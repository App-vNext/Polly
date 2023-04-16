using Polly.Strategy;

namespace Polly.Telemetry;

internal static class TelemetryUtil
{
    private const string PollyDiagnosticSource = "Polly";

    private static readonly DiagnosticSource DefaultDiagnosticSource = new DiagnosticListener(PollyDiagnosticSource);

    private static readonly ResiliencePropertyKey<DiagnosticSource> DiagnosticSourceKey = new("DiagnosticSource");

    public static ResilienceStrategyTelemetry CreateTelemetry(string builderName, ResilienceProperties builderProperties, string strategyName, string strategyType)
    {
        // Allows the user to override the default diagnostic source.
        if (!builderProperties.TryGetValue(DiagnosticSourceKey, out var diagnosticSource))
        {
            diagnosticSource = DefaultDiagnosticSource;
        }

        var telemetrySource = new ResilienceTelemetrySource(builderName, builderProperties, strategyName, strategyType);

        return new ResilienceStrategyTelemetry(telemetrySource, diagnosticSource);
    }
}
