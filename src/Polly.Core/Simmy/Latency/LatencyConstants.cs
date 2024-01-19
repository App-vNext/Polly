namespace Polly.Simmy.Latency;

internal static class LatencyConstants
{
    public const string OnLatencyInjectedEvent = "OnLatencyInjected";

    public static readonly TimeSpan DefaultLatency = TimeSpan.FromSeconds(30);
}
