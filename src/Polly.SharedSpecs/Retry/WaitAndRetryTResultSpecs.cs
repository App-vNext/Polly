using FluentAssertions;
using Polly.Duration;
using Polly.Specs.Helpers;
using Polly.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Polly.Specs.Retry
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class WaitAndRetryTResultSpecs : IDisposable
    {
        public WaitAndRetryTResultSpecs()
        {
            // do nothing on call to sleep
            SystemClock.Sleep = (_, __) => { };
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
        {
            Dictionary<ResultPrimitive, TimeSpan> expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>(){

                {ResultPrimitive.Fault, 2.Seconds()},
                {ResultPrimitive.FaultAgain, 4.Seconds()},
            };

            List<TimeSpan> actualRetryWaits = new List<TimeSpan>();

            Polly.Retry.RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .WaitAndRetry(2,
                    (retryAttempt, outcome, ctx) => expectedRetryWaits[outcome.Result],
                    (_, timeSpan, __, ___) => actualRetryWaits.Add(timeSpan)
                );

            using (Dictionary<ResultPrimitive, TimeSpan>.Enumerator enumerator = expectedRetryWaits.GetEnumerator())
            {
                policy.Execute(() =>
                {
                    if (enumerator.MoveNext()) return enumerator.Current.Key;
                    else return ResultPrimitive.Undefined;
                });
            }

            actualRetryWaits.Should().ContainInOrder(expectedRetryWaits.Values);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_linear_strategy()
        {
            LinearBackoff durationStrategy = new LinearBackoff(TimeSpan.FromSeconds(1), 2, false);

            // Discrete

            TimeSpan[] expectedDiscrete = new TimeSpan[5]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(7),
                TimeSpan.FromSeconds(9)
            };

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete(5);
            actualDurations.Should().ContainInOrder(expectedDiscrete);

            // Take

            TimeSpan[] expectedContinuous = new TimeSpan[7]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(7),
                TimeSpan.FromSeconds(9),

                TimeSpan.FromSeconds(9),
                TimeSpan.FromSeconds(9)
            };

            actualDurations = durationStrategy.Take(5, 7).ToArray();
            actualDurations.Should().ContainInOrder(expectedContinuous);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_linear_strategy_fastfirst()
        {
            LinearBackoff durationStrategy = new LinearBackoff(TimeSpan.FromSeconds(1), 2, true);

            // Discrete

            TimeSpan[] expectedDurations = new TimeSpan[5]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(7)
            };

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete(5);
            actualDurations.Should().ContainInOrder(expectedDurations);

            // Take

            TimeSpan[] expectedContinuous = new TimeSpan[7]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(7),

                TimeSpan.FromSeconds(7),
                TimeSpan.FromSeconds(7)
            };

            actualDurations = durationStrategy.Take(5, 7).ToArray();
            actualDurations.Should().ContainInOrder(expectedContinuous);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_exponential_strategy()
        {
            ExponentialBackoff durationStrategy = new ExponentialBackoff(TimeSpan.FromSeconds(1), false);

            // Discrete

            TimeSpan[] expectedDurations = new TimeSpan[5]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(16)
            };

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete(5);
            actualDurations.Should().ContainInOrder(expectedDurations);

            // Take

            TimeSpan[] expectedContinuous = new TimeSpan[7]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(16),

                TimeSpan.FromSeconds(16),
                TimeSpan.FromSeconds(16)
            };

            actualDurations = durationStrategy.Take(5, 7).ToArray();
            actualDurations.Should().ContainInOrder(expectedContinuous);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_exponential_strategy_fastfirst()
        {
            ExponentialBackoff durationStrategy = new ExponentialBackoff(TimeSpan.FromSeconds(1), true);

            // Discrete

            TimeSpan[] expectedDurations = new TimeSpan[5]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8)
            };

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete(5);
            actualDurations.Should().ContainInOrder(expectedDurations);

            // Take

            TimeSpan[] expectedContinuous = new TimeSpan[7]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),

                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(8)
            };

            actualDurations = durationStrategy.Take(5, 7).ToArray();
            actualDurations.Should().ContainInOrder(expectedContinuous);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_jitter_strategy()
        {
            const int count = 20;
            const int take = 5;
            DecorrelatedJitterBackoff durationStrategy = new DecorrelatedJitterBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), false);

            // Discrete

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete(count);
            actualDurations.Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);

            // Take

            actualDurations = durationStrategy.Take(count, count + take).ToArray();

            IEnumerable<TimeSpan> extra = actualDurations.Skip(count);
            IEnumerable<TimeSpan> discrete = actualDurations.Take(count);
            TimeSpan max = discrete.Max();

            discrete.Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);
            extra.Should().OnlyContain(n => n == max);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_jitter_strategy_fastfirst()
        {
            const int count = 20;
            const int take = 5;
            DecorrelatedJitterBackoff durationStrategy = new DecorrelatedJitterBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), true);

            // Discrete

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete(count);
            actualDurations.Take(1).Should().OnlyContain(n => n == TimeSpan.Zero);
            actualDurations.Skip(1).Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);

            // Take

            actualDurations = durationStrategy.Take(count, count + take).ToArray();
            actualDurations.Take(1).Should().OnlyContain(n => n == TimeSpan.Zero);

            IEnumerable<TimeSpan> extra = actualDurations.Skip(count);
            IEnumerable<TimeSpan> discrete = actualDurations.Skip(1).Take(count - 1);
            TimeSpan max = discrete.Max();

            discrete.Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);
            extra.Should().OnlyContain(n => n == max);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}