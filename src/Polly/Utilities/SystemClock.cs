using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Utilities
{
    /// <summary>
    /// Time related delegates used to support different compilation targets and to improve testability of the code.
    /// </summary>
    public static class SystemClock
    {
        /// <summary>
        /// Allows the setting of a custom Thread.Sleep implementation for testing.
        /// By default this will use the <see cref="CancellationToken"/>'s <see cref="WaitHandle"/>.
        /// </summary>
        public static Action<TimeSpan, CancellationToken> Sleep = (timeSpan, cancellationToken) =>
        {
            if (cancellationToken.WaitHandle.WaitOne(timeSpan)) cancellationToken.ThrowIfCancellationRequested();
        };

        /// <summary>
        /// Allows the setting of a custom async Sleep implementation for testing.
        /// By default this will be a call to <see cref="M:Task.Delay"/> taking a <see cref="CancellationToken"/>
        /// </summary>
#if NET40
        public static Func<TimeSpan, CancellationToken, Task> SleepAsync = TaskEx.Delay;
#else
        public static Func<TimeSpan, CancellationToken, Task> SleepAsync = Task.Delay;
#endif
        /// <summary>
        /// Allows the setting of a custom DateTime.UtcNow implementation for testing.
        /// By default this will be a call to <see cref="DateTime.UtcNow"/>
        /// </summary>
        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;

        /// <summary>
        /// Allows the setting of a custom DateTimeOffset.UtcNow implementation for testing.
        /// By default this will be a call to <see cref="DateTime.UtcNow"/>
        /// </summary>
        public static Func<DateTimeOffset> DateTimeOffsetUtcNow = () => DateTimeOffset.UtcNow;

        /// <summary>
        /// Allows the setting of a custom method for cancelling tokens after a timespan, for use in testing.
        /// By default this will be a call to CancellationTokenSource.CancelAfter(timespan)
        /// </summary>
        public static Action<CancellationTokenSource, TimeSpan> CancelTokenAfter = (tokenSource, timespan) => tokenSource.CancelAfter(timespan);

        /// <summary>
        /// Resets the custom implementations to their defaults. 
        /// Should be called during test teardowns.
        /// </summary>
        public static void Reset()
        {
            Sleep = (timeSpan, cancellationToken) =>
            {
                if (cancellationToken.WaitHandle.WaitOne(timeSpan)) cancellationToken.ThrowIfCancellationRequested();
            };

#if NET40
            SleepAsync = TaskEx.Delay;
#else
            SleepAsync = Task.Delay;
#endif
            UtcNow = () => DateTime.UtcNow;

            DateTimeOffsetUtcNow = () => DateTimeOffset.UtcNow;

            CancelTokenAfter = (tokenSource, timespan) => tokenSource.CancelAfter(timespan);

        }
    }
}