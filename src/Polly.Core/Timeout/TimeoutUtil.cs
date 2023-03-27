using System.Globalization;

namespace Polly.Timeout;

internal static class TimeoutUtil
{
    public const string TimeSpanInvalidMessage = "The '{0}' must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout).";

    public static bool ShouldApplyTimeout(TimeSpan timeout)
    {
        return timeout > TimeSpan.Zero;
    }

    public static bool IsTimeoutValid(TimeSpan timeout)
    {
        if (timeout > TimeSpan.Zero)
        {
            return true;
        }

        return timeout == TimeoutStrategyOptions.InfiniteTimeout || timeout > TimeSpan.Zero;
    }

    public static void ValidateTimeout(TimeSpan timeout)
    {
        if (!IsTimeoutValid(timeout))
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                timeout,
                string.Format(CultureInfo.InvariantCulture, TimeSpanInvalidMessage, timeout));
        }
    }
}
