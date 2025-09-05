using Microsoft.Extensions.Logging;

namespace Polly.Telemetry;

internal static class TelemetryUtil
{
    private const int MaxIntegers = 100;

    private static readonly object[] Integers = [.. Enumerable.Range(0, MaxIntegers).Select(v => (object)v)];

    private static readonly object True = true;

    private static readonly object False = false;

    internal const string PollyDiagnosticSource = "Polly";

    public static string GetValueOrPlaceholder(this string? value) => value ?? "(null)";

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
        ResilienceEventSeverity.Debug => LogLevel.Debug,
        ResilienceEventSeverity.Information => LogLevel.Information,
        ResilienceEventSeverity.Warning => LogLevel.Warning,
        ResilienceEventSeverity.Error => LogLevel.Error,
        ResilienceEventSeverity.Critical => LogLevel.Critical,
        _ => LogLevel.None,
    };

    public static string AsString(this ResilienceEventSeverity severity) => severity switch
    {
        ResilienceEventSeverity.None => nameof(ResilienceEventSeverity.None),
        ResilienceEventSeverity.Debug => nameof(ResilienceEventSeverity.Debug),
        ResilienceEventSeverity.Information => nameof(ResilienceEventSeverity.Information),
        ResilienceEventSeverity.Warning => nameof(ResilienceEventSeverity.Warning),
        ResilienceEventSeverity.Error => nameof(ResilienceEventSeverity.Error),
        ResilienceEventSeverity.Critical => nameof(ResilienceEventSeverity.Critical),
        _ => severity.ToString(),
    };
}
