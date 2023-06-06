namespace Polly.Extensions.Utils;

internal static class TelemetryUtil
{
    internal const string PollyDiagnosticSource = "Polly";

    internal static readonly ResiliencePropertyKey<string> StrategyKey = new("Polly.StrategyKey");
}
