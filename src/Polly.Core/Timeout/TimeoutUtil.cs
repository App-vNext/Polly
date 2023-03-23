namespace Polly.Timeout;

internal static class TimeoutUtil
{
    public static bool ShouldApplyTimeout(TimeSpan timeout)
    {
        return timeout > TimeSpan.Zero;
    }

    public static void ValidateTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero && timeout != TimeoutConstants.InfiniteTimeout)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                timeout,
                $"The '{nameof(timeout)}' must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout).");
        }
    }
}
