namespace Polly.Simmy.Latency;

internal static class LatencyConstants
{
    public const string OnDelayedEvent = "OnDelayed";

    public static readonly TimeSpan DefaultLatency = TimeSpan.FromSeconds(30);
}
