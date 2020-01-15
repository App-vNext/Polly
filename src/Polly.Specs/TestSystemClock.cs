using Polly.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs
{
    internal sealed class TestSystemClock : ISystemClock
    {
        // Track a CancellationTokenSource, and when it might be cancelled at.
        private CancellationTokenSource _trackedTokenSource = null;
        private DateTimeOffset _cancelAt = DateTimeOffset.MaxValue;

        public TestSystemClock()
            : this(DateTimeOffset.UtcNow)
        { }

        public TestSystemClock(DateTimeOffset utcNow)
        {
            DateTimeOffsetUtcNow = utcNow;
        }

        public DateTime UtcNow { get => DateTimeOffsetUtcNow.UtcDateTime; set => DateTimeOffsetUtcNow = value; }

        public DateTimeOffset DateTimeOffsetUtcNow { get; set; }

        public void CancelTokenAfter(CancellationTokenSource tokenSource, TimeSpan delay)
        {
            if (_trackedTokenSource != null && tokenSource != _trackedTokenSource) throw new InvalidOperationException("Timeout tests cannot track more than one timing out token at a time.");

            _trackedTokenSource = tokenSource;

            var newCancelAt = DateTimeOffsetUtcNow.Add(delay);
            _cancelAt = newCancelAt < _cancelAt ? newCancelAt : _cancelAt;

            Sleep(TimeSpan.Zero, CancellationToken.None); // Invoke our custom definition of sleep, to check for immediate cancellation.
        }

        public void Sleep(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            if (_trackedTokenSource == null || _trackedTokenSource.IsCancellationRequested)
            {
                // Not tracking any CancellationToken (or already cancelled) - just advance time.
                DateTimeOffsetUtcNow += timeout;
            }
            else
            {
                // Tracking something to cancel - does this sleep hit time to cancel?
                var timeToCancellation = _cancelAt - DateTimeOffsetUtcNow;
                if (timeout >= timeToCancellation)
                {
                    // Cancel!  (And advance time only to the instant of cancellation)
                    DateTimeOffsetUtcNow += timeToCancellation;

                    // (and stop tracking it after cancelling; it can't be cancelled twice, so there is no need, and the owner may dispose it)
                    var copySource = _trackedTokenSource;
                    _trackedTokenSource = null;
                    copySource.Cancel();
                    copySource.Token.ThrowIfCancellationRequested();
                }
                else
                {
                    // (not yet time to cancel - just advance time)
                    DateTimeOffsetUtcNow += timeout;
                }
            }
        }

        public Task SleepAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            Sleep(delay, cancellationToken);
            return Task.FromResult(true);
        }
    }
}
