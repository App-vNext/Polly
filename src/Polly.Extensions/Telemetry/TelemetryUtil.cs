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
}
