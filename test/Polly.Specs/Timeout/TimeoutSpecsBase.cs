using System.Runtime.CompilerServices;

namespace Polly.Specs.Timeout;

/// <summary>
/// Provides common functionality for timeout specs, which abstracts out both SystemClock.Sleep, and SystemClock.CancelTokenAfter.
/// <remarks>Polly's TimeoutPolicy uses timing-out CancellationTokens to drive timeouts.
/// For tests, rather than letting .NET's timers drive the timing out of CancellationTokens, we override SystemClock.CancelTokenAfter and SystemClock.Sleep to make the tests run fast.
/// </remarks>
/// </summary>
public abstract class TimeoutSpecsBase : IDisposable
{
    // xUnit creates a new class instance per test, so these variables are isolated per test.

    // Track a CancellationTokenSource, and when it might be cancelled at.
    private CancellationTokenSource? _trackedTokenSource;
    private DateTimeOffset _cancelAt = DateTimeOffset.MaxValue;

    private DateTimeOffset _offsetUtcNow = DateTimeOffset.UtcNow;
    private DateTime _utcNow = DateTime.UtcNow;

    protected TimeoutSpecsBase()
    {
        // Override the SystemClock, to return time stored in variables we manipulate.
        SystemClock.DateTimeOffsetUtcNow = () => _offsetUtcNow;
        SystemClock.UtcNow = () => _utcNow;

        // Override SystemClock.CancelTokenAfter to record when the policy wants the token to cancel.
        SystemClock.CancelTokenAfter = (tokenSource, timespan) =>
        {
            if (_trackedTokenSource != null && tokenSource != _trackedTokenSource)
            {
                throw new InvalidOperationException("Timeout tests cannot track more than one timing out token at a time.");
            }

            _trackedTokenSource = tokenSource;

            DateTimeOffset newCancelAt = _offsetUtcNow.Add(timespan);
            _cancelAt = newCancelAt < _cancelAt ? newCancelAt : _cancelAt;

            SystemClock.Sleep(TimeSpan.Zero, CancellationToken.None); // Invoke our custom definition of sleep, to check for immediate cancellation.
        };

        // Override SystemClock.Sleep, to manipulate our artificial clock.  And - if it means sleeping beyond the time when a tracked token should cancel - cancel it!
        SystemClock.Sleep = (sleepTimespan, sleepCancellationToken) =>
        {
            if (sleepCancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (_trackedTokenSource == null || _trackedTokenSource.IsCancellationRequested)
            {
                // Not tracking any CancellationToken (or already cancelled) - just advance time.
                _utcNow += sleepTimespan;
                _offsetUtcNow += sleepTimespan;
            }
            else
            {
                // Tracking something to cancel - does this sleep hit time to cancel?
                TimeSpan timeToCancellation = _cancelAt - _offsetUtcNow;
                if (sleepTimespan >= timeToCancellation)
                {
                    // Cancel!  (And advance time only to the instant of cancellation)
                    _offsetUtcNow += timeToCancellation;
                    _utcNow += timeToCancellation;

                    // (and stop tracking it after cancelling; it can't be cancelled twice, so there is no need, and the owner may dispose it)
                    CancellationTokenSource copySource = _trackedTokenSource;
                    _trackedTokenSource = null;
                    copySource.Cancel();
                    copySource.Token.ThrowIfCancellationRequested();
                }
                else
                {
                    // (not yet time to cancel - just advance time)
                    _utcNow += sleepTimespan;
                    _offsetUtcNow += sleepTimespan;
                }
            }
        };

        SystemClock.SleepAsync = (sleepTimespan, cancellationToken) =>
        {
            SystemClock.Sleep(sleepTimespan, cancellationToken);
            return Task.FromResult(true);
        };
    }

    public void Dispose() =>
        SystemClock.Reset();

    /// <summary>
    /// A helper method which simply throws the passed exception.  Supports tests verifying the stack trace of where an exception was thrown, by throwing that exception from a specific (other) location.
    /// </summary>
    /// <param name="ex">The exception to throw.</param>
    [MethodImpl(MethodImplOptions.NoInlining)] // Tests that use this method assert that the exception was thrown from within this method; therefore, it is essential that
    protected static void Helper_ThrowException(Exception ex) =>
        throw ex;
}
