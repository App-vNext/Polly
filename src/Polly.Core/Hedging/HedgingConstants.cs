namespace Polly.Hedging;

internal static class HedgingConstants
{
    public const string StrategyType = "Hedging";

    public const string OnHedgingEventName = "OnHedging";

    public const int DefaultMaxHedgedAttempts = 2;

    public const int MinimumHedgedAttempts = 2;

    public const int MaximumHedgedAttempts = 10;

    public static readonly TimeSpan DefaultHedgingDelay = TimeSpan.FromSeconds(2);
}
