namespace Polly.Specs.RateLimit;

public abstract class RateLimitPolicySpecsBase : RateLimitSpecsBase
{
    protected abstract IRateLimitPolicy GetPolicyViaSyntax(
        int numberOfExecutions,
        TimeSpan perTimeSpan);

    protected abstract IRateLimitPolicy GetPolicyViaSyntax(
        int numberOfExecutions,
        TimeSpan perTimeSpan,
        int maxBurst);

    protected abstract (bool PermitExecution, TimeSpan RetryAfter) TryExecuteThroughPolicy(IRateLimitPolicy policy);

    protected void ShouldPermitAnExecution(IRateLimitPolicy policy)
    {
        (bool permitExecution, TimeSpan retryAfter) = TryExecuteThroughPolicy(policy);

        permitExecution.Should().BeTrue();
        retryAfter.Should().Be(TimeSpan.Zero);
    }

    protected void ShouldPermitNExecutions(IRateLimitPolicy policy, long numberOfExecutions)
    {
        for (int execution = 0; execution < numberOfExecutions; execution++)
        {
            ShouldPermitAnExecution(policy);
        }
    }

    protected void ShouldNotPermitAnExecution(IRateLimitPolicy policy, TimeSpan? retryAfter = null)
    {
        (bool PermitExecution, TimeSpan RetryAfter) canExecute = TryExecuteThroughPolicy(policy);

        canExecute.PermitExecution.Should().BeFalse();
        if (retryAfter == null)
        {
            canExecute.RetryAfter.Should().BeGreaterThan(TimeSpan.Zero);
        }
        else
        {
            canExecute.RetryAfter.Should().Be(retryAfter.Value);
        }
    }

    [Fact]
    public void Syntax_should_throw_for_perTimeSpan_zero()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.Zero);

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("perTimeSpan");
    }

    [Fact]
    public void Syntax_should_throw_for_perTimeSpan_infinite()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(1, System.Threading.Timeout.InfiniteTimeSpan);

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("perTimeSpan");
    }

    [Fact]
    public void Syntax_should_throw_for_perTimeSpan_too_small()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(int.MaxValue, TimeSpan.FromSeconds(1));

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("perTimeSpan");
        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.Message.Should().StartWith("The number of executions per timespan must be positive.");
    }

    [Fact]
    public void Syntax_should_throw_for_numberOfExecutions_negative()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(-1, TimeSpan.FromSeconds(1));

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("numberOfExecutions");
    }

    [Fact]
    public void Syntax_should_throw_for_numberOfExecutions_zero()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(0, TimeSpan.FromSeconds(1));

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("numberOfExecutions");
    }

    [Fact]
    public void Syntax_should_throw_for_perTimeSpan_negative()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.FromTicks(-1));

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("perTimeSpan");
    }

    [Fact]
    public void Syntax_should_throw_for_maxBurst_negative()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.FromSeconds(1), -1);

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("maxBurst");
    }

    [Fact]
    public void Syntax_should_throw_for_maxBurst_zero()
    {
        Action invalidSyntax = () => GetPolicyViaSyntax(1, TimeSpan.FromSeconds(1), 0);

        invalidSyntax.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("maxBurst");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void Given_bucket_capacity_one_and_time_not_advanced_ratelimiter_specifies_correct_wait_until_next_execution(int onePerSeconds)
    {
        FixClock();

        // Arrange
        TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
        var rateLimiter = GetPolicyViaSyntax(1, onePer);

        // Assert - first execution after initialising should always be permitted.
        ShouldPermitAnExecution(rateLimiter);

        // Arrange
        // (do nothing - time not advanced)

        // Assert - should be blocked - time not advanced.
        ShouldNotPermitAnExecution(rateLimiter, onePer);
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
        var rateLimiter = GetPolicyViaSyntax(1, onePer, bucketCapacity);

        // Act - should be able to successfully take bucketCapacity items.
        ShouldPermitNExecutions(rateLimiter, bucketCapacity);

        // Assert - should not be able to take any items (given time not advanced).
        ShouldNotPermitAnExecution(rateLimiter, onePer);
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
        var rateLimiter = GetPolicyViaSyntax(1, onePer, bucketCapacity);

        // Arrange - spend the initial bucket capacity.
        ShouldPermitNExecutions(rateLimiter, bucketCapacity);
        ShouldNotPermitAnExecution(rateLimiter);

        // Act-Assert - repeatedly advance the clock towards the interval but not quite - then to the interval
        int experimentRepeats = bucketCapacity * 3;
        TimeSpan shortfallFromInterval = TimeSpan.FromTicks(1);
        TimeSpan notQuiteInterval = onePer - shortfallFromInterval;
        for (int i = 0; i < experimentRepeats; i++)
        {
            // Arrange - Advance clock not quite to the interval
            AdvanceClock(notQuiteInterval.Ticks);

            // Assert - should not quite be able to issue another token
            ShouldNotPermitAnExecution(rateLimiter, shortfallFromInterval);

            // Arrange - Advance clock to the interval
            AdvanceClock(shortfallFromInterval.Ticks);

            // Act
            ShouldPermitAnExecution(rateLimiter);

            // Assert - but cannot get another token straight away
            ShouldNotPermitAnExecution(rateLimiter);
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
        var rateLimiter = GetPolicyViaSyntax(1, onePer, bucketCapacity);

        // Arrange - spend the initial bucket capacity.
        ShouldPermitNExecutions(rateLimiter, bucketCapacity);
        ShouldNotPermitAnExecution(rateLimiter);

        // Arrange - advance exactly enough to permit a full bucket burst
        AdvanceClock(onePer.Ticks * bucketCapacity);

        // Assert - expect full bucket capacity but no more
        ShouldPermitNExecutions(rateLimiter, bucketCapacity);
        ShouldNotPermitAnExecution(rateLimiter);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    public void Given_any_bucket_capacity_rate_limiter_permits_half_full_bucket_burst_after_half_required_refill_time_elapsed(int bucketCapacity)
    {
        (bucketCapacity % 2).Should().Be(0);

        FixClock();

        // Arrange
        int onePerSeconds = 1;
        TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
        var rateLimiter = GetPolicyViaSyntax(1, onePer, bucketCapacity);

        // Arrange - spend the initial bucket capacity.
        ShouldPermitNExecutions(rateLimiter, bucketCapacity);
        ShouldNotPermitAnExecution(rateLimiter);

        // Arrange - advance multiple times enough to permit a full bucket burst
        AdvanceClock(onePer.Ticks * (bucketCapacity / 2));

        // Assert - expect full bucket capacity but no more
        ShouldPermitNExecutions(rateLimiter, bucketCapacity / 2);
        ShouldNotPermitAnExecution(rateLimiter);
    }

    [Theory]
    [InlineData(100, 2)]
    [InlineData(100, 5)]
    public void Given_any_bucket_capacity_rate_limiter_permits_only_full_bucket_burst_even_if_multiple_required_refill_time_elapsed(int bucketCapacity, int multipleRefillTimePassed)
    {
        multipleRefillTimePassed.Should().BeGreaterThan(1);

        FixClock();

        // Arrange
        int onePerSeconds = 1;
        TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
        var rateLimiter = GetPolicyViaSyntax(1, onePer, bucketCapacity);

        // Arrange - spend the initial bucket capacity.
        ShouldPermitNExecutions(rateLimiter, bucketCapacity);
        ShouldNotPermitAnExecution(rateLimiter);

        // Arrange - advance multiple times enough to permit a full bucket burst
        AdvanceClock(onePer.Ticks * bucketCapacity * multipleRefillTimePassed);

        // Assert - expect full bucket capacity but no more
        ShouldPermitNExecutions(rateLimiter, bucketCapacity);
        ShouldNotPermitAnExecution(rateLimiter);
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
        var rateLimiter = GetPolicyViaSyntax(1, onePer);

        // Arrange - parallel tasks all waiting on a manual reset event.
        using var gate = new ManualResetEventSlim();
        Task<(bool PermitExecution, TimeSpan RetryAfter)>[] tasks = new Task<(bool, TimeSpan)>[parallelContention];
        for (int i = 0; i < parallelContention; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                gate.Wait();
                return TryExecuteThroughPolicy(rateLimiter);
            });
        }

        // Act - release gate.
        gate.Set();
#pragma warning disable S6603
        RateLimitSpecsBase.Within(TimeSpan.FromSeconds(10 /* high to allow for slow-running on time-slicing CI servers */), () => tasks.All(t => t.IsCompleted).Should().BeTrue());
#pragma warning restore S6603

        // Assert - one should have permitted execution, n-1 not.
        var results = tasks.Select(t => t.Result).ToList();
        results.Count(r => r.PermitExecution).Should().Be(1);
        results.Count(r => !r.PermitExecution).Should().Be(parallelContention - 1);
    }
}
