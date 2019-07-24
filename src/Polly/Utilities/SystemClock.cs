using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Utilities
{
    /// <inheritdoc/>
    public sealed class SystemClock : ISystemClock
    {
        private SystemClock() { }

        /// <summary>
        /// Gets the default <see cref="ISystemClock"/> instance.
        /// </summary>
        public static ISystemClock Default { get; } = new SystemClock();

        /// <summary>
        /// Gets or sets the ambient system clock.
        /// </summary>
        public static ISystemClock Current { get; set; } = Default;

        /// <summary>
        /// Resets the ambient system clock to the default.
        /// Should be called during test teardowns.
        /// </summary>
        public static void Reset() => Current = Default;

        /// <inheritdoc/>
        public void Sleep(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (cancellationToken.WaitHandle.WaitOne(timeout)) cancellationToken.ThrowIfCancellationRequested();
        }

        /// <inheritdoc/>
        public Task SleepAsync(TimeSpan delay, CancellationToken cancellationToken) => Task.Delay(delay, cancellationToken);

        /// <inheritdoc/>
        public DateTime UtcNow => DateTime.UtcNow;

        /// <inheritdoc/>
        public DateTimeOffset DateTimeOffsetUtcNow => DateTimeOffset.UtcNow;

        /// <inheritdoc/>
        public void CancelTokenAfter(CancellationTokenSource tokenSource, TimeSpan delay) => tokenSource.CancelAfter(delay);
    }
}