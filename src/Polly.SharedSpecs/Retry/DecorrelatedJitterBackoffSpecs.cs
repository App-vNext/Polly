using FluentAssertions;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Polly.Specs.Retry
{
    public static class DecorrelatedJitterBackoffSpecs
    {
        private const int Seed = 123456789; // Specific seed for determinism

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_identical_sequence_when_same_random(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff1 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, Seed);
            DecorrelatedJitterBackoff backoff2 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, Seed);
            IEnumerable<TimeSpan> discrete1 = backoff1.Generate(count);
            IEnumerable<TimeSpan> discrete2 = backoff2.Generate(count);

            discrete1.Should().HaveCount(count);
            discrete2.Should().HaveCount(count);
            discrete1.Should().ContainInOrder(discrete2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_different_sequence_when_different_random(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff1 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, 123);
            DecorrelatedJitterBackoff backoff2 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, 321);
            IEnumerable<TimeSpan> discrete1 = backoff1.Generate(count);
            IEnumerable<TimeSpan> discrete2 = backoff2.Generate(count);

            discrete1.Should().HaveCount(count);
            discrete2.Should().HaveCount(count);

            if (fastFirst)
                discrete1.First().Should().Be(discrete2.First());

            double sum1 = discrete1.Sum(n => n.TotalMilliseconds);
            double sum2 = discrete2.Sum(n => n.TotalMilliseconds);
            sum1.Should().NotBe(sum2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_different_sequence_with_null_random(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff1 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst);
            DecorrelatedJitterBackoff backoff2 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst);
            IEnumerable<TimeSpan> discrete1 = backoff1.Generate(count);
            IEnumerable<TimeSpan> discrete2 = backoff2.Generate(count);

            discrete1.Should().HaveCount(count);
            discrete2.Should().HaveCount(count);

            if (fastFirst)
                discrete1.First().Should().Be(discrete2.First());

            double sum1 = discrete1.Sum(n => n.TotalMilliseconds);
            double sum2 = discrete2.Sum(n => n.TotalMilliseconds);
            sum1.Should().NotBe(sum2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_small(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, Seed);
            IEnumerable<TimeSpan> discrete = backoff.Generate(count);

            discrete.Should().HaveCount(count);

            if (fastFirst)
            {
                discrete.First().Should().Be(TimeSpan.Zero);
                discrete = discrete.Skip(1);
            }

            discrete.Should().Contain(n => n >= minDelay && n <= maxDelay);

            int groupCount = discrete
                .Select(n => n.TotalMilliseconds)
                .GroupBy(n => n)
                .Count();

            groupCount.Should().BeGreaterThan(400); // 463
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_large(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(150_000);

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, Seed);
            IEnumerable<TimeSpan> discrete = backoff.Generate(count);

            discrete.Should().HaveCount(count);

            if (fastFirst)
            {
                discrete.First().Should().Be(TimeSpan.Zero);
                discrete = discrete.Skip(1);
            }

            discrete.Should().Contain(n => n >= minDelay && n <= maxDelay);

            int groupCount = discrete
                .Select(n => n.TotalMilliseconds)
                .GroupBy(n => n)
                .Count();

            groupCount.Should().BeGreaterThan(700); // 733
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_no_variance_when_range_zero(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(0);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(0);

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, Seed);
            IEnumerable<TimeSpan> discrete = backoff.Generate(count);

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
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, Seed);
            IEnumerable<TimeSpan> discrete = backoff.Generate(count);

            discrete.Should().BeEmpty();
        }

        [Fact]
        public static void Should_be_able_to_calculate_retry_timespans()
        {
            const int count = 20;
            DecorrelatedJitterBackoff durationStrategy = new DecorrelatedJitterBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), false);

            IEnumerable<TimeSpan> actualDurations = durationStrategy.Generate(count);
            actualDurations.Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);
        }

        [Fact]
        public static void Should_be_able_to_calculate_retry_timespans_fastfirst()
        {
            const int count = 20;
            DecorrelatedJitterBackoff durationStrategy = new DecorrelatedJitterBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), true);

            IEnumerable<TimeSpan> actualDurations = durationStrategy.Generate(count);
            actualDurations.Take(1).Should().OnlyContain(n => n == TimeSpan.Zero);
            actualDurations.Skip(1).Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);
        }
    }
}
