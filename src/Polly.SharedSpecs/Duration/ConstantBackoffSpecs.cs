using FluentAssertions;
using Polly.Duration;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Polly.Specs.Duration
{
    public static class ConstantBackoffSpecs
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_identical_sequence_when_same_params(bool fastFirst)
        {
            const int count = 20;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ConstantBackoff backoff1 = new ConstantBackoff(count, minDelay, fastFirst);
            ConstantBackoff backoff2 = new ConstantBackoff(count, minDelay, fastFirst);
            IReadOnlyList<TimeSpan> discrete1 = backoff1.Discrete();
            IReadOnlyList<TimeSpan> discrete2 = backoff2.Discrete();

            discrete1.Should().ContainInOrder(discrete2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_small(bool fastFirst)
        {
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ConstantBackoff backoff = new ConstantBackoff(20, minDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

            if (fastFirst)
            {
                discrete.First().Should().Be(TimeSpan.Zero);
                discrete = discrete.Skip(1);
            }

            discrete.Should().Contain(n => n >= minDelay);

            int groupCount = discrete
                .Select(n => n.TotalMilliseconds)
                .GroupBy(n => n)
                .Count();

            groupCount.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_large(bool fastFirst)
        {
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ConstantBackoff backoff = new ConstantBackoff(20, minDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

            if (fastFirst)
            {
                discrete.First().Should().Be(TimeSpan.Zero);
                discrete = discrete.Skip(1);
            }

            discrete.Should().Contain(n => n >= minDelay);

            int groupCount = discrete
                .Select(n => n.TotalMilliseconds)
                .GroupBy(n => n)
                .Count();

            groupCount.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_no_variance_when_range_zero(bool fastFirst)
        {
            TimeSpan minDelay = TimeSpan.FromMilliseconds(0);

            ConstantBackoff backoff = new ConstantBackoff(20, minDelay, fastFirst);
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

            ConstantBackoff backoff = new ConstantBackoff(0, minDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

            discrete.Should().BeEmpty();
        }
    }
}
