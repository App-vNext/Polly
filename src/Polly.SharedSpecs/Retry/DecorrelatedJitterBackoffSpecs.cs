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
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_identical_sequence_when_same_explicit_seed(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            const int seed = 123456789;
            DecorrelatedJitterBackoff backoff1 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, seed);
            DecorrelatedJitterBackoff backoff2 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, seed);
            IEnumerable<TimeSpan> discrete1 = backoff1.GetSleepDurations(count).ToList();
            IEnumerable<TimeSpan> discrete2 = backoff2.GetSleepDurations(count).ToList();

            // Experiment
            IEnumerable<TimeSpan> discrete3 = Alternative.DecorrelatedJitterBackoff(minDelay, maxDelay, count, fastFirst, seed).ToList();
            discrete3.Should().HaveCount(count);

            discrete1.Should().HaveCount(count);
            discrete2.Should().HaveCount(count);
            discrete1.Should().ContainInOrder(discrete2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Should_have_different_sequence_when_different_seed(bool fastFirst)
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff1 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, 123);
            DecorrelatedJitterBackoff backoff2 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst, 321);
            IEnumerable<TimeSpan> discrete1 = backoff1.GetSleepDurations(count).ToList();
            IEnumerable<TimeSpan> discrete2 = backoff2.GetSleepDurations(count).ToList();

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
        public static void Should_have_different_sequence_with_default_seed(bool fastFirst) // Same _implicit_ seed
        {
            const int count = 1000;
            TimeSpan minDelay = TimeSpan.FromMilliseconds(10);
            TimeSpan maxDelay = TimeSpan.FromMilliseconds(1500);

            DecorrelatedJitterBackoff backoff1 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst);
            DecorrelatedJitterBackoff backoff2 = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst);
            IEnumerable<TimeSpan> discrete1 = backoff1.GetSleepDurations(count).ToList();
            IEnumerable<TimeSpan> discrete2 = backoff2.GetSleepDurations(count).ToList();

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

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count).ToList();

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

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count).ToList();

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

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count).ToList();

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

            DecorrelatedJitterBackoff backoff = new DecorrelatedJitterBackoff(minDelay, maxDelay, fastFirst);
            IEnumerable<TimeSpan> discrete = backoff.GetSleepDurations(count).ToList();

            discrete.Should().BeEmpty();
        }

        [Fact]
        public static void Should_be_able_to_calculate_retry_timespans()
        {
            const int count = 20;
            DecorrelatedJitterBackoff durationStrategy = new DecorrelatedJitterBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), false);

            IEnumerable<TimeSpan> actualDurations = durationStrategy.GetSleepDurations(count).ToList();
            actualDurations.Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);
        }

        [Fact]
        public static void Should_be_able_to_calculate_retry_timespans_fastfirst()
        {
            const int count = 20;
            DecorrelatedJitterBackoff durationStrategy = new DecorrelatedJitterBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), true);

            IEnumerable<TimeSpan> actualDurations = durationStrategy.GetSleepDurations(count).ToList();
            actualDurations.Take(1).Should().OnlyContain(n => n == TimeSpan.Zero);
            actualDurations.Skip(1).Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);
        }

        [Fact]
        public static void A_single_instance_of_IEnumerable_returned_from_Generate_method_should_generate_different_results_each_time_it_is_enumerated()
        {
            const int count = 20;
            DecorrelatedJitterBackoff durationStrategy = new DecorrelatedJitterBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));

            // Take an instance of IEnumerable<TimeSpan> as directly returned by DecorrelatedJitterBackoff.Generate(...), exactly as users should when configuring a policy.
            IEnumerable<TimeSpan> generate = durationStrategy.GetSleepDurations(count);

            // Check: We definitely only have a single instance of - we are repeatedly using the same instance of - IEnumerable<TimeSpan>
            generate.Should().BeSameAs(generate); // BeSameAs(...) in FluentAssertions means refer to exact same instance in memory.

            // Check: If enumerate that same instance twice, we get _different_ reified enumerated sequences.
            generate.SequenceEqual(generate).Should().BeFalse(); // Checks the reified sequences generated by enumerating twice. 

            // A more explicit version of the preceding assertion, for clarity:
            generate.ToList().SequenceEqual(generate.ToList()).Should().BeFalse(); // Twice, reifies the sequence into a list; the two generated lists differ.

            // But as the below statements show, users should not reify the list before passing the IEnumerable to the retry policy.  
            // That would indeed cause the same sequence of timespans to be reused every time the policy was used.
            IEnumerable<TimeSpan> reifiedList = generate.ToList();
            reifiedList.SequenceEqual(reifiedList).Should().BeTrue();
        }
    }
}
