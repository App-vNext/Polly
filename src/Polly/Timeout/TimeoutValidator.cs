using System;

namespace Polly.Timeout
{
    internal static class TimeoutValidator
    {

        internal static readonly TimeSpan InfiniteTimeSpan = System.Threading.Timeout.InfiniteTimeSpan;

        internal static void ValidateSecondsTimeout(int seconds)
        {
            if (seconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds));
            }
        }

        internal static void ValidateTimeSpanTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero && timeout != InfiniteTimeSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout,
                    $"{nameof(timeout)} must be a positive TimeSpan (or Timeout.InfiniteTimeSpan to indicate no timeout)");
            }
        }

    }
}
