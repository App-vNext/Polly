namespace Polly.Simmy.Latency;

internal static class ChaosLatencyConstants
{
    public const string DefaultName = "Chaos.Latency";

    public const string OnLatencyInjectedEvent = "Chaos.OnLatency";

    public static readonly TimeSpan DefaultLatency = TimeSpan.FromSeconds(30);
}
