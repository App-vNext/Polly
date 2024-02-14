namespace Polly.Timeout;

internal static class TimeoutUtil
{
    public const string TimeSpanInvalidMessage = "The '{0}' must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout).";

    public static bool ShouldApplyTimeout(TimeSpan timeout) => timeout > TimeSpan.Zero;
}
