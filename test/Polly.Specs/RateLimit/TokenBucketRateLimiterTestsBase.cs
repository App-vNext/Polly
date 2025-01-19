namespace Polly.Specs.RateLimit;

[Collection(Constants.SystemClockDependentTestCollection)]
public abstract class TokenBucketRateLimiterTestsBase : RateLimitSpecsBase, IDisposable
{
    internal abstract IRateLimiter GetRateLimiter(TimeSpan onePer, long bucketCapacity);

    public void Dispose() =>
        SystemClock.Reset();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void Given_bucket_capacity_one_and_time_not_advanced_ratelimiter_specifies_correct_wait_until_next_execution(int onePerSeconds)
    {
        FixClock();

        // Arrange
        TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
        var rateLimiter = GetRateLimiter(onePer, 1);

        // Assert - first execution after initialising should always be permitted.
        rateLimiter.ShouldPermitAnExecution();

        // Arrange
        // (do nothing - time not advanced)

        // Assert - should be blocked - time not advanced.
        rateLimiter.ShouldNotPermitAnExecution(onePer);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(50)]
    public void Given_bucket_capacity_N_and_time_not_advanced_ratelimiter_permits_executions_up_to_bucket_capacity(int bucketCapacity)
    {
        FixClock();

        // Arrange.
        TimeSpan onePer = TimeSpan.FromSeconds(1);
        var rateLimiter = GetRateLimiter(onePer, bucketCapacity);

        // Act - should be able to successfully take bucketCapacity items.
        rateLimiter.ShouldPermitNExecutions(bucketCapacity);

        // Assert - should not be able to take any items (given time not advanced).
        rateLimiter.ShouldNotPermitAnExecution(onePer);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(5, 1)]
    [InlineData(1, 10)]
    [InlineData(2, 10)]
    [InlineData(5, 10)]
    public void Given_any_bucket_capacity_ratelimiter_permits_another_execution_per_interval(int onePerSeconds, int bucketCapacity)
    {
        FixClock();

        // Arrange
        TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
        var rateLimiter = GetRateLimiter(onePer, bucketCapacity);

        // Arrange - spend the initial bucket capacity.
        rateLimiter.ShouldPermitNExecutions(bucketCapacity);
        rateLimiter.ShouldNotPermitAnExecution();

        // Act-Assert - repeatedly advance the clock towards the interval but not quite - then to the interval
        int experimentRepeats = bucketCapacity * 3;
        TimeSpan shortfallFromInterval = TimeSpan.FromTicks(1);
        TimeSpan notQuiteInterval = onePer - shortfallFromInterval;
        for (int i = 0; i < experimentRepeats; i++)
        {
            // Arrange - Advance clock not quite to the interval
            AdvanceClock(notQuiteInterval.Ticks);

            // Assert - should not quite be able to issue another token
            rateLimiter.ShouldNotPermitAnExecution(shortfallFromInterval);

            // Arrange - Advance clock to the interval
            AdvanceClock(shortfallFromInterval.Ticks);

            // Act
            rateLimiter.ShouldPermitAnExecution();

            // Assert - but cannot get another token straight away
            rateLimiter.ShouldNotPermitAnExecution();
        }
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    public void Given_any_bucket_capacity_rate_limiter_permits_full_bucket_burst_after_exact_elapsed_time(int bucketCapacity)
    {
        FixClock();

        // Arrange
        int onePerSeconds = 1;
        TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
        var rateLimiter = GetRateLimiter(onePer, bucketCapacity);

        // Arrange - spend the initial bucket capacity.
        rateLimiter.ShouldPermitNExecutions(bucketCapacity);
        rateLimiter.ShouldNotPermitAnExecution();

        // Arrange - advance exactly enough to permit a full bucket burst
        AdvanceClock(onePer.Ticks * bucketCapacity);

        // Assert - expect full bucket capacity but no more
        rateLimiter.ShouldPermitNExecutions(bucketCapacity);
        rateLimiter.ShouldNotPermitAnExecution();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    public void Given_any_bucket_capacity_rate_limiter_permits_half_full_bucket_burst_after_half_required_refill_time_elapsed(int bucketCapacity)
    {
        (bucketCapacity % 2).ShouldBe(0);

        FixClock();

        // Arrange
        int onePerSeconds = 1;
        TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
        var rateLimiter = GetRateLimiter(onePer, bucketCapacity);

        // Arrange - spend the initial bucket capacity.
        rateLimiter.ShouldPermitNExecutions(bucketCapacity);
        rateLimiter.ShouldNotPermitAnExecution();

        // Arrange - advance multiple times enough to permit a full bucket burst
        AdvanceClock(onePer.Ticks * (bucketCapacity / 2));

        // Assert - expect full bucket capacity but no more
        rateLimiter.ShouldPermitNExecutions(bucketCapacity / 2);
        rateLimiter.ShouldNotPermitAnExecution();
    }

    [Theory]
    [InlineData(100, 2)]
    [InlineData(100, 5)]
    public void Given_any_bucket_capacity_rate_limiter_permits_only_full_bucket_burst_even_if_multiple_required_refill_time_elapsed(int bucketCapacity, int multipleRefillTimePassed)
    {
        multipleRefillTimePassed.ShouldBeGreaterThan(1);

        FixClock();

        // Arrange
        int onePerSeconds = 1;
        TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
        var rateLimiter = GetRateLimiter(onePer, bucketCapacity);

        // Arrange - spend the initial bucket capacity.
        rateLimiter.ShouldPermitNExecutions(bucketCapacity);
        rateLimiter.ShouldNotPermitAnExecution();

        // Arrange - advance multiple times enough to permit a full bucket burst
        AdvanceClock(onePer.Ticks * bucketCapacity * multipleRefillTimePassed);

        // Assert - expect full bucket capacity but no more
        rateLimiter.ShouldPermitNExecutions(bucketCapacity);
        rateLimiter.ShouldNotPermitAnExecution();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(100)]
    public void Given_immediate_parallel_contention_ratelimiter_still_only_permits_one(int parallelContention)
    {
        FixClock();

        // Arrange
        TimeSpan onePer = TimeSpan.FromSeconds(1);
        var rateLimiter = GetRateLimiter(onePer, 1);

        // Arrange - parallel tasks all waiting on a manual reset event.
        using var gate = new ManualResetEventSlim();
        var tasks = new Task<(bool PermitExecution, TimeSpan RetryAfter)>[parallelContention];
        for (int i = 0; i < parallelContention; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                gate.Wait();
                return rateLimiter.PermitExecution();
            });
        }

        // Act - release gate.
        gate.Set();

        Within(
            TimeSpan.FromSeconds(10 /* high to allow for slow-running on time-slicing CI servers */),
            () => Assert.All(tasks, (t) => Assert.True(t.IsCompleted)));

        // Assert - one should have permitted execution, n-1 not.
        var results = tasks.Select(t => t.Result).ToList();
        results.Count(r => r.PermitExecution).ShouldBe(1);
        results.Count(r => !r.PermitExecution).ShouldBe(parallelContention - 1);
    }
}
