﻿using FluentAssertions;
using Polly.Duration;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Polly.Specs.Duration
{
    public static class LinearBackoffSpecs
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_identical_sequence_when_same_factor(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            LinearBackoff backoff1 = new LinearBackoff(count, minDelay, 1.0, fastFirst);
            LinearBackoff backoff2 = new LinearBackoff(count, minDelay, 1.0, fastFirst);
            var discrete1 = backoff1.Discrete();
            var discrete2 = backoff2.Discrete();

            discrete1.Should().ContainInOrder(discrete2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_different_sequence_when_different_factor(bool fastFirst)
        {
            const int count = 100;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            LinearBackoff backoff1 = new LinearBackoff(count, minDelay, 1.0, fastFirst);
            LinearBackoff backoff2 = new LinearBackoff(count, minDelay, 2.0, fastFirst);
            var discrete1 = backoff1.Discrete();
            var discrete2 = backoff2.Discrete();

            if (fastFirst)
            {
                discrete1[0].Should().Be(discrete2[0]);
                discrete1[1].Should().Be(discrete2[1]);
            }
            else
            {
                discrete1[0].Should().Be(discrete2[0]);
            }

            discrete1[2].Should().NotBe(discrete2[2]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_an_adequate_variance_when_range_small(bool fastFirst)
        {
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);

            LinearBackoff backoff = new LinearBackoff(1000, minDelay, 1.0, fastFirst);
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

            LinearBackoff backoff = new LinearBackoff(1000, minDelay, 1.0, fastFirst);
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

            LinearBackoff backoff = new LinearBackoff(1000, minDelay, 1.0, fastFirst);
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

            LinearBackoff backoff = new LinearBackoff(0, minDelay, 1.0, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.Discrete();

            discrete.Should().BeEmpty();
        }
    }
}
