using FluentAssertions;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Polly.Specs.Retry
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

            ConstantBackoff backoff1 = new ConstantBackoff(minDelay, fastFirst);
            ConstantBackoff backoff2 = new ConstantBackoff(minDelay, fastFirst);
            IEnumerable<TimeSpan> discrete1 = backoff1.GetSleepDurations(count);
            IEnumerable<TimeSpan> discrete2 = backoff2.GetSleepDurations(count);

            discrete1.Should().HaveCount(count);
            discrete2.Should().HaveCount(count);
            discrete1.Should().ContainInOrder(discrete2);

            // Factory-based instantiation
            IEnumerable<TimeSpan> discrete3 = ConstantBackoff.Create(minDelay, count, fastFirst).ToList();
            discrete3.Should().HaveCount(count);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_duration_specified(bool fastFirst)
        {
            const int count = 20;
            TimeSpan delay = TimeSpan.FromMilliseconds(5);

            ConstantBackoff backoff = new ConstantBackoff(delay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count);

            discrete.Should().HaveCount(count);

            if (fastFirst)
            {
                discrete.First().Should().Be(TimeSpan.Zero);
                discrete = discrete.Skip(1);
            }

            discrete.Should().Contain(n => n == delay);

            int groupCount = discrete
                .Select(n => n.TotalMilliseconds)
                .GroupBy(n => n)
                .Count();

            groupCount.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_no_variance_when_duration_zero(bool fastFirst)
        {
            const int count = 20;
            TimeSpan delay = TimeSpan.FromMilliseconds(0);

            ConstantBackoff backoff = new ConstantBackoff(delay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count);

            discrete.Should().HaveCount(count);

            if (fastFirst)
            {
                discrete.First().Should().Be(TimeSpan.Zero);
                discrete = discrete.Skip(1);
            }

            discrete.Should().Contain(n => n == delay);

            int groupCount = discrete
                .Select(n => n.TotalMilliseconds)
                .GroupBy(n => n)
                .Count();

            groupCount.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_be_empty_when_count_zero(bool fastFirst)
        {
            const int count = 0;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            ConstantBackoff backoff = new ConstantBackoff(minDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count);

            discrete.Should().BeEmpty();
        }
    }
}
