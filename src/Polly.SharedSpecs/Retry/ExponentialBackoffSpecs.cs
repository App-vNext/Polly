using FluentAssertions;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Polly.Specs.Retry
{
    public static class ExponentialBackoffSpecs
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_identical_sequence_when_same_params(bool fastFirst)
        {
            const int count = 10;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ExponentialBackoff backoff1 = new ExponentialBackoff(minDelay, fastFirst: fastFirst);
            ExponentialBackoff backoff2 = new ExponentialBackoff(minDelay, fastFirst: fastFirst);
            IEnumerable<TimeSpan> discrete1 = backoff1.GetSleepDurations(count);
            IEnumerable<TimeSpan> discrete2 = backoff2.GetSleepDurations(count);

            discrete1.Should().HaveCount(count);
            discrete2.Should().HaveCount(count);

            discrete1.Should().ContainInOrder(discrete2);
        }

        [Theory]
        [InlineData(1.0, false, 10 * 5)]
        [InlineData(1.0, true, 10 * 4)]
        [InlineData(2.0, false, 10 + 20 + 40 + 80 + 160)]
        [InlineData(2.0, true, 10 + 20 + 40 + 80)]
        [InlineData(3.0, false, 10 + 30 + 90 + 270 + 810)]
        [InlineData(3.0, true, 10 + 30 + 90 + 270)]
        public static void Should_have_different_sequence_when_different_factors(double factor, bool fastFirst, double sum)
        {
            const int count = 5;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ExponentialBackoff backoff1 = new ExponentialBackoff(minDelay, factor, fastFirst);
            IEnumerable<TimeSpan> discrete1 = backoff1.GetSleepDurations(count);

            discrete1.Should().HaveCount(count);
            discrete1.Sum(n => n.TotalMilliseconds).Should().Be(sum);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_small(bool fastFirst)
        {
            const int count = 20;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ExponentialBackoff backoff = new ExponentialBackoff(minDelay, fastFirst: fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count);

            discrete.Should().HaveCount(count);

            int expectedCount = count;
            if (fastFirst)
            {
                discrete.First().Should().Be(TimeSpan.Zero);
                discrete = discrete.Skip(1);
                expectedCount--;
            }

            discrete.Should().Contain(n => n >= minDelay);

            int groupCount = discrete
                .Select(n => n.TotalMilliseconds)
                .GroupBy(n => n)
                .Count();

            groupCount.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_large(bool fastFirst)
        {
            const int count = 20;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ExponentialBackoff backoff = new ExponentialBackoff(minDelay, fastFirst: fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count);

            discrete.Should().HaveCount(count);

            int expectedCount = count;
            if (fastFirst)
            {
                discrete.First().Should().Be(TimeSpan.Zero);
                discrete = discrete.Skip(1);
                expectedCount--;
            }

            discrete.Should().Contain(n => n >= minDelay);

            int groupCount = discrete
                .Select(n => n.TotalMilliseconds)
                .GroupBy(n => n)
                .Count();

            groupCount.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_no_variance_when_range_zero(bool fastFirst)
        {
            const int count = 20;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(0);

            ExponentialBackoff backoff = new ExponentialBackoff(minDelay, fastFirst: fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count);

            discrete.Should().HaveCount(count);

            if (fastFirst)
            {
                discrete.First().Should().Be(TimeSpan.Zero);
                discrete = discrete.Skip(1);
            }

            discrete.Should().Contain(n => n == minDelay);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_be_empty_when_count_zero(bool fastFirst)
        {
            const int count = 0;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ExponentialBackoff backoff = new ExponentialBackoff(minDelay, fastFirst: fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count);

            discrete.Should().BeEmpty();
        }

        [Fact]
        public static void Should_be_able_to_calculate_retry_timespans()
        {
            ExponentialBackoff durationStrategy = new ExponentialBackoff(TimeSpan.FromSeconds(1), fastFirst: false);

            TimeSpan[] expectedDurations = new TimeSpan[5]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(16)
            };

            IEnumerable<TimeSpan> actualDurations = durationStrategy.GetSleepDurations(5);
            actualDurations.Should().ContainInOrder(expectedDurations);
        }

        [Fact]
        public static void Should_be_able_to_calculate_retry_timespans_fastfirst()
        {
            ExponentialBackoff durationStrategy = new ExponentialBackoff(TimeSpan.FromSeconds(1), fastFirst: true);

            TimeSpan[] expectedDurations = new TimeSpan[5]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8)
            };

            IEnumerable<TimeSpan> actualDurations = durationStrategy.GetSleepDurations(5);
            actualDurations.Should().ContainInOrder(expectedDurations);
        }
    }
}
