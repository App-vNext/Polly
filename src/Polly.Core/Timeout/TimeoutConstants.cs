namespace Polly.Timeout;

internal static class TimeoutConstants
{
    public const string StrategyType = "Timeout";

    public const string OnTimeoutEvent = "OnTimeout";

    public static readonly TimeSpan InfiniteTimeout = System.Threading.Timeout.InfiniteTimeSpan;
}
