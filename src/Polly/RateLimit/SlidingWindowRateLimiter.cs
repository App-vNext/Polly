using System;
using System.Collections.Concurrent;

namespace Polly.RateLimit;

/// <summary>
/// An IRateLimiter implementation for a Polly <see cref="IRateLimitPolicy"/> that uses sliding window that limits
/// the number of executions for a specified window of time.
/// </summary>
internal sealed class SlidingWindowRateLimiter : IRateLimiter
{
    private readonly TimeSpan _window;
    private readonly int _maxExecution;
    private readonly ConcurrentQueue<DateTime> _windowQueue;

    /// <summary>
    /// Creates an instance of <see cref="SlidingWindowRateLimiter"/>
    /// </summary>
    /// <param name="window">The window time frame for limiting the number of executions</param>
    /// <param name="maxExecution">The maximum number of execution allowed per time frame window specified</param>
    public SlidingWindowRateLimiter(TimeSpan window, int maxExecution)
    {
        if (window <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(window), window, $"The {nameof(LockFreeTokenBucketRateLimiter)} must specify a positive TimeSpan for a window of time.");
        
        _window = window;
        _maxExecution = maxExecution;
        _windowQueue = new ConcurrentQueue<DateTime>();
    }

    /// <summary>
    /// Returns whether the execution is permitted.
    /// If not, returns the amount of time <see cref="TimeSpan"/> should be waited before retrying.
    /// </summary>
    /// <returns>Tuple of permit execution boolean, and the amount of time to wait before retrying</returns>
    public (bool permitExecution, TimeSpan retryAfter) PermitExecution()
    {
        var utcNow = DateTime.UtcNow;
        if (_windowQueue.Count == 0)
        {
            _windowQueue.Enqueue(utcNow);
            return (true, TimeSpan.Zero);
        }

        if (_windowQueue.Count == _maxExecution)
        {
            _windowQueue.TryDequeue(out var earliestExecution);
            if (utcNow - earliestExecution <= _window)
            {
                _windowQueue.TryPeek(out var nextExecution);
                var retryAfter = nextExecution - earliestExecution;
                _windowQueue.Enqueue(earliestExecution);
                return (false, retryAfter);
            }
        }

        _windowQueue.Enqueue(utcNow);
        return (true, TimeSpan.Zero);
    }
}