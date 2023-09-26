﻿namespace Polly.Simmy.Latency;

internal static class LatencyConstants
{
    public const string OnLatencyEvent = "OnLatency";

    public static readonly TimeSpan DefaultLatency = TimeSpan.FromSeconds(30);
}
