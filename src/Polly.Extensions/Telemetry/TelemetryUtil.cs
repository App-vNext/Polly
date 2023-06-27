using Microsoft.Extensions.Logging;
using Polly.Telemetry;

namespace Polly.Extensions.Telemetry;

internal static class TelemetryUtil
{
    private const int MaxIntegers = 100;

    private static readonly object[] Integers = Enumerable.Range(0, MaxIntegers).Select(v => (object)v).ToArray();

    private static readonly object True = true;

    private static readonly object False = false;

    internal const string PollyDiagnosticSource = "Polly";

    internal static readonly ResiliencePropertyKey<string> StrategyKey = new("Polly.StrategyKey");

    public static object AsBoxedBool(this bool value) => value switch
    {
        true => True,
        false => False,
    };

    public static object AsBoxedInt(this int value) => value switch
    {
        >= 0 and < MaxIntegers => Integers[value],
        _ => value,
    };

    public static LogLevel AsLogLevel(this ResilienceEventSeverity severity) => severity switch
    {
        ResilienceEventSeverity.Information => LogLevel.Information,
        ResilienceEventSeverity.Warning => LogLevel.Warning,
        ResilienceEventSeverity.Error => LogLevel.Error,
        _ => LogLevel.Information,
    };

    public static string AsString(this ResilienceEventSeverity severity) => severity switch
    {
        ResilienceEventSeverity.Information => nameof(ResilienceEventSeverity.Information),
        ResilienceEventSeverity.Warning => nameof(ResilienceEventSeverity.Warning),
        ResilienceEventSeverity.Error => nameof(ResilienceEventSeverity.Error),
        _ => severity.ToString(),
    };
}
