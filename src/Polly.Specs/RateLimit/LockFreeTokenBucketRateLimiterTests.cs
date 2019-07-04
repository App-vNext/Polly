﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.RateLimit;
using Polly.Utilities;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Sdk;

namespace Polly.Specs.RateLimit
{

    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]

    public class LockFreeTokenBucketRateLimiterTests : IDisposable
    {
        public IRateLimiter GetRateLimiter(TimeSpan onePer, long bucketCapacity)
            => new LockFreeTokenBucketRateLimiter(onePer, bucketCapacity);

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

            // Act - first execution after initialising should always be permitted.
            (bool permitExecution, TimeSpan retryAfter) canExecute = rateLimiter.PermitExecution();

            // Assert.
            canExecute.permitExecution.Should().BeTrue();
            canExecute.retryAfter.Should().Be(TimeSpan.Zero);

            // Arrange
            // (do nothing - time not advanced)

            // Act - try another execution
            canExecute = rateLimiter.PermitExecution();

            // Assert - should be blocked - time not advanced.
            canExecute.permitExecution.Should().BeFalse();
            canExecute.retryAfter.Should().Be(onePer);
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
            AssertCanTakeNItems(rateLimiter, bucketCapacity);

            // After that, should not be able to take any items (given time not advanced).

            // Act
            (bool permitExecution, TimeSpan retryAfter) canExecute = rateLimiter.PermitExecution();

            // Assert - should be blocked - time not advanced.
            canExecute.permitExecution.Should().BeFalse();
            canExecute.retryAfter.Should().BeGreaterThan(TimeSpan.Zero);
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
            // Arrange
            TimeSpan onePer = TimeSpan.FromSeconds(onePerSeconds);
            var rateLimiter = GetRateLimiter(onePer, bucketCapacity);

            FixClock();

            // Arrange - spend the initial bucket capacity.
            AssertCanTakeNItems(rateLimiter, bucketCapacity);

            // Act-Assert - repeatedly advance the clock towards the interval but not quite - then to the interval
            int experimentRepeats = bucketCapacity * 3;
            const int shortfallFromInterval = 1;
            long notQuiteInterval = onePer.Ticks - shortfallFromInterval;
            (bool permitExecution, TimeSpan retryAfter) canExecute;
            for (int i = 0; i < experimentRepeats; i++)
            {
                // Arrange - Advance clock not quite to the interval
                AdvanceClock(notQuiteInterval);
                // Act
                canExecute = rateLimiter.PermitExecution();
                // Assert - should not quite be able to issue another token
                canExecute.permitExecution.Should().BeFalse();
                canExecute.retryAfter.Should().Be(TimeSpan.FromTicks(shortfallFromInterval));

                // Arrange - Advance clock to the interval
                AdvanceClock(shortfallFromInterval);
                // Act
                canExecute = rateLimiter.PermitExecution();
                // Assert - should be able to issue another token
                canExecute.permitExecution.Should().BeTrue();
                canExecute.retryAfter.Should().Be(TimeSpan.Zero);

                // Act
                canExecute = rateLimiter.PermitExecution();
                // Assert - cannot get another token straight away
                canExecute.permitExecution.Should().BeFalse();
                canExecute.retryAfter.Should().BeGreaterThan(TimeSpan.Zero);
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(100)]
        public void Given_immediate_parallel_contention_ratelimiter_still_only_permits_one(int parallelContention)
        {
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
            Within(TimeSpan.FromSeconds(5), () => tasks.All(t => t.IsCompleted).Should().BeTrue());

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
            TimeSpan retryInterval = TimeSpan.FromTicks(Math.Min(TimeSpan.FromSeconds(0.2).Ticks, timeSpan.Ticks/10));

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

        private static void AssertCanTakeNItems(IRateLimiter rateLimiter, long expectedTake)
        {
            (bool permitExecution, TimeSpan retryAfter) canExecute;
            for (int take = 0; take < expectedTake; take++)
            {
                // Act
                canExecute = rateLimiter.PermitExecution();

                // Assert.
                canExecute.permitExecution.Should().BeTrue();
                canExecute.retryAfter.Should().Be(TimeSpan.Zero);
            }
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }

    }
}
