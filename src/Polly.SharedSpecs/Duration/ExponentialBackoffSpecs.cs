using FluentAssertions;
using Polly.Duration;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Polly.Specs.Duration
{
    public static class ExponentialBackoffSpecs
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_identical_sequence_when_same_factor(bool fastFirst)
        {
            const int count = 20;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ExponentialBackoff backoff1 = new ExponentialBackoff(count, minDelay, fastFirst);
            ExponentialBackoff backoff2 = new ExponentialBackoff(count, minDelay, fastFirst);
            var discrete1 = backoff1.Discrete();
            var discrete2 = backoff2.Discrete();

            discrete1.Should().ContainInOrder(discrete2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_small(bool fastFirst)
        {
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ExponentialBackoff backoff = new ExponentialBackoff(20, minDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

            int expectedCount = backoff.RetryCount;
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
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ExponentialBackoff backoff = new ExponentialBackoff(20, minDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

            int expectedCount = backoff.RetryCount;
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
            TimeSpan minDelay = TimeSpan.FromMilliseconds(0);

            ExponentialBackoff backoff = new ExponentialBackoff(20, minDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

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
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ExponentialBackoff backoff = new ExponentialBackoff(0, minDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

            discrete.Should().BeEmpty();
        }
    }
}
