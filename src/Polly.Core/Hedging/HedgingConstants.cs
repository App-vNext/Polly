namespace Polly.Hedging;

internal static class HedgingConstants
{
    public const string DefaultName = "Hedging";

    public const string OnHedgingEventName = "OnHedging";

    public const int DefaultMaxHedgedAttempts = 1;

    public const int MinimumHedgedAttempts = 1;

    public const int MaximumHedgedAttempts = 10;

    public static readonly TimeSpan DefaultHedgingDelay = TimeSpan.FromSeconds(2);
}
