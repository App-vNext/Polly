namespace Polly.Extensions.Telemetry;

internal static class TelemetryUtil
{
    internal const string PollyDiagnosticSource = "Polly";

    internal static readonly ResiliencePropertyKey<string> StrategyKey = new("Polly.StrategyKey");

    public static string AsResultString(this bool value) => value switch
    {
        true => "Success",
        false => "Failure",
    };
}
