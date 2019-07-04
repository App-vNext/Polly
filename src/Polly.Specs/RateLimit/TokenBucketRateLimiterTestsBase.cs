using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Polly.RateLimit;
using Polly.Specs.Helpers.RateLimit;
using Polly.Utilities;
using Xunit;
using Xunit.Sdk;

namespace Polly.Specs.RateLimit
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public abstract class TokenBucketRateLimiterTestsBase : IDisposable
    {
        public abstract IRateLimiter GetRateLimiter(TimeSpan onePer, long bucketCapacity);

        public void Dispose()
        {
            SystemClock.Reset();
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
            var rateLimiter = GetRateLimiter(onePer, 1);

            // Assert - first execution after initialising should always be permitted.
            rateLimiter.ShouldPermitAnExecution();

            // Arrange
            // (do nothing - time not advanced)

            // Assert - should be blocked - time not advanced.
            rateLimiter.ShouldNotPermitAnExecution();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public void Given_bucket_capacity_N_and_time_not_advanced_ratelimiter_permits_executions_up_to_bucket_capacity(long bucketCapacity)
        {
            FixClock();

            // Arrange.
            TimeSpan onePer = TimeSpan.FromSeconds(1);
            var rateLimiter = GetRateLimiter(onePer, bucketCapacity);

            // Act - should be able to successfully take bucketCapacity items.
            rateLimiter.ShouldPermitNExecutions(bucketCapacity);

            // Assert - should not be able to take any items (given time not advanced).
            rateLimiter.ShouldNotPermitAnExecution();
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
            ManualResetEventSlim gate = new ManualResetEventSlim();
            Task<(bool permitExecution, TimeSpan retryAfter)>[] tasks = new Task<(bool, TimeSpan)>[parallelContention];
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
            Within(TimeSpan.FromSeconds(10 /* high to allow for slow-running on time-slicing CI servers */), () => tasks.All(t => t.IsCompleted).Should().BeTrue());

            // Assert - one should have permitted execution, n-1 not.
            var results = tasks.Select(t => t.Result).ToList();
            results.Count(r => r.permitExecution).Should().Be(1);
            results.Count(r => !r.permitExecution).Should().Be(parallelContention - 1);
        }

        /// <summary>
        /// Asserts that the actionContainingAssertions will succeed without <see cref="AssertionFailedException"/> or <see cref="XunitException"/>, within the given timespan.  Checks are made each time a status-change pulse is received from the <see cref="TraceableAction"/>s executing through the bulkhead.
        /// </summary>
        /// <param name="timeSpan">The allowable timespan.</param>
        /// <param name="actionContainingAssertions">The action containing fluent assertions, which must succeed within the timespan.</param>
        private void Within(TimeSpan timeSpan, Action actionContainingAssertions)
        {
            TimeSpan retryInterval = TimeSpan.FromTicks(Math.Min(TimeSpan.FromSeconds(0.2).Ticks, timeSpan.Ticks / 10));

            Stopwatch watch = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    actionContainingAssertions.Invoke();
                    break;
                }
                catch (Exception e)
                {
                    if (!(e is AssertionFailedException || e is XunitException)) { throw; }

                    if (watch.Elapsed > timeSpan) { throw; }

                    Thread.Sleep(retryInterval);
                }
            }
        }

        private static void FixClock()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            SystemClock.DateTimeOffsetUtcNow = () => now;
        }

        private static void AdvanceClock(long advanceTicks)
        {
            DateTimeOffset now = SystemClock.DateTimeOffsetUtcNow();
            SystemClock.DateTimeOffsetUtcNow = () => now + TimeSpan.FromTicks(advanceTicks);
        }
    }
}
