using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Utilities
{
    /// <summary>
    /// Time related delegates used to support different compilation targets and to improve testability of the code.
    /// </summary>
    public interface ISystemClock
    {
        /// <summary>
        /// Allows the setting of a custom Thread.Sleep implementation for testing.
        /// By default this will use the <see cref="CancellationToken"/>'s <see cref="WaitHandle"/>.
        /// </summary>
        void Sleep(TimeSpan timeout, CancellationToken cancellationToken);

        /// <summary>
        /// Allows the setting of a custom async Sleep implementation for testing.
        /// By default this will be a call to <see cref="M:Task.Delay"/> taking a <see cref="CancellationToken"/>
        /// </summary>
        Task SleepAsync(TimeSpan delay, CancellationToken cancellationToken);

        /// <summary>
        /// Allows the setting of a custom DateTime.UtcNow implementation for testing.
        /// By default this will be a call to <see cref="DateTime.UtcNow"/>
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// Allows the setting of a custom DateTimeOffset.UtcNow implementation for testing.
        /// By default this will be a call to <see cref="DateTime.UtcNow"/>
        /// </summary>
        DateTimeOffset DateTimeOffsetUtcNow { get; }

        /// <summary>
        /// Allows the setting of a custom method for cancelling tokens after a timespan, for use in testing.
        /// By default this will be a call to CancellationTokenSource.CancelAfter(timespan)
        /// </summary>
        void CancelTokenAfter(CancellationTokenSource tokenSource, TimeSpan delay);
    }
}