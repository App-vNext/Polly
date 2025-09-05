namespace Polly.Specs.RateLimit;

public abstract class RateLimitSpecsBase
{
    /// <summary>
    /// Asserts that the actionContainingAssertions will succeed without <see cref="AssertionFailedException"/> or <see cref="XunitException"/>, within the given timespan.  Checks are made each time a status-change pulse is received from the <see cref="TraceableAction"/>s executing through the bulkhead.
    /// </summary>
    /// <param name="timeSpan">The allowable timespan.</param>
    /// <param name="actionContainingAssertions">The action containing fluent assertions, which must succeed within the timespan.</param>
    protected static void Within(TimeSpan timeSpan, Action actionContainingAssertions)
    {
        TimeSpan retryInterval = TimeSpan.FromSeconds(0.2);

        Stopwatch watch = Stopwatch.StartNew();
        var token = TestCancellation.Token;

        while (!token.IsCancellationRequested)
        {
            try
            {
                actionContainingAssertions.Invoke();
                break;
            }
            catch (Exception e)
            {
                if (e is not IAssertionException)
                {
                    throw;
                }

                if (watch.Elapsed > timeSpan)
                {
                    throw;
                }

                Thread.Sleep(retryInterval);
            }
        }

        token.ThrowIfCancellationRequested();
    }

    protected static void FixClock()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        SystemClock.DateTimeOffsetUtcNow = () => now;
    }

    protected static void AdvanceClock(TimeSpan advance)
    {
        DateTimeOffset now = SystemClock.DateTimeOffsetUtcNow();
        SystemClock.DateTimeOffsetUtcNow = () => now + advance;
    }

    protected static void AdvanceClock(long advanceTicks) =>
        AdvanceClock(TimeSpan.FromTicks(advanceTicks));
}
