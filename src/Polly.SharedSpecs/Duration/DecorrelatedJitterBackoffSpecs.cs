﻿using FluentAssertions;
using Polly.Duration;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Polly.Specs.Duration
{
    public static class DecorrelatedJitterBackoffSpecs
    {
        private static readonly Random s_uniform = new Random(123456789); // Specific seed for determinism

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_identical_sequence_when_same_random(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff1 = new DecorrelatedJitterBackoff(count, minDelay, maxDelay, fastFirst, new Random(123456789));
            DecorrelatedJitterBackoff backoff2 = new DecorrelatedJitterBackoff(count, minDelay, maxDelay, fastFirst, new Random(123456789));
            var discrete1 = backoff1.Discrete();
            var discrete2 = backoff2.Discrete();

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

            DecorrelatedJitterBackoff backoff1 = new DecorrelatedJitterBackoff(count, minDelay, maxDelay, fastFirst, new Random(123));
            DecorrelatedJitterBackoff backoff2 = new DecorrelatedJitterBackoff(count, minDelay, maxDelay, fastFirst, new Random(321));
            var discrete1 = backoff1.Discrete();
            var discrete2 = backoff2.Discrete();

            if (fastFirst)
                discrete1[0].Should().Be(discrete2[0]);
            else
                discrete1[0].Should().NotBe(discrete2[0]);

            discrete1[1].Should().NotBe(discrete2[1]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_different_sequence_with_null_random(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff1 = new DecorrelatedJitterBackoff(count, minDelay, maxDelay, fastFirst);
            DecorrelatedJitterBackoff backoff2 = new DecorrelatedJitterBackoff(count, minDelay, maxDelay, fastFirst);
            var discrete1 = backoff1.Discrete();
            var discrete2 = backoff2.Discrete();

            if (fastFirst)
                discrete1[0].Should().Be(discrete2[0]);
            else
                discrete1[0].Should().NotBe(discrete2[0]);

            discrete1[1].Should().NotBe(discrete2[1]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_small(bool fastFirst)
        {
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(1000, minDelay, maxDelay, fastFirst, s_uniform);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

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

            groupCount.Should().BeGreaterThan(700); // 730
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_large(bool fastFirst)
        {
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(150_000);

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(1000, minDelay, maxDelay, fastFirst, s_uniform);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

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

            groupCount.Should().BeGreaterThan(900); // 994
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_no_variance_when_range_zero(bool fastFirst)
        {
            TimeSpan minDelay = TimeSpan.FromMilliseconds(0);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(0);

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(1000, minDelay, maxDelay, fastFirst, s_uniform);
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
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(0, minDelay, maxDelay, fastFirst, s_uniform);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

            discrete.Should().BeEmpty();
        }
    }
}
