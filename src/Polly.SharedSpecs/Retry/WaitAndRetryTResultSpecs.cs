using FluentAssertions;
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
            Duration.LinearBackoff durationStrategy = new Duration.LinearBackoff(5, TimeSpan.FromSeconds(1), 2, false);

            // Discrete

            TimeSpan[] expectedDiscrete = new TimeSpan[5]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(7),
                TimeSpan.FromSeconds(9)
            };

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete();
            actualDurations.Should().ContainInOrder(expectedDiscrete);

            // Continuous

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

            actualDurations = durationStrategy.Continuous(7).ToArray();
            actualDurations.Should().ContainInOrder(expectedContinuous);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_linear_strategy_fastfirst()
        {
            Duration.LinearBackoff durationStrategy = new Duration.LinearBackoff(5, TimeSpan.FromSeconds(1), 2, true);

            // Discrete

            TimeSpan[] expectedDurations = new TimeSpan[5]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(7)
            };

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete();
            actualDurations.Should().ContainInOrder(expectedDurations);

            // Continuous

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

            actualDurations = durationStrategy.Continuous(7).ToArray();
            actualDurations.Should().ContainInOrder(expectedContinuous);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_exponential_strategy()
        {
            Duration.ExponentialBackoff durationStrategy = new Duration.ExponentialBackoff(5, TimeSpan.FromSeconds(1), false);

            // Discrete

            TimeSpan[] expectedDurations = new TimeSpan[5]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(16)
            };

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete();
            actualDurations.Should().ContainInOrder(expectedDurations);

            // Continuous

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

            actualDurations = durationStrategy.Continuous(7).ToArray();
            actualDurations.Should().ContainInOrder(expectedContinuous);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_exponential_strategy_fastfirst()
        {
            Duration.ExponentialBackoff durationStrategy = new Duration.ExponentialBackoff(5, TimeSpan.FromSeconds(1), true);

            // Discrete

            TimeSpan[] expectedDurations = new TimeSpan[5]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8)
            };

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete();
            actualDurations.Should().ContainInOrder(expectedDurations);

            // Continuous

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

            actualDurations = durationStrategy.Continuous(7).ToArray();
            actualDurations.Should().ContainInOrder(expectedContinuous);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_jitter_strategy()
        {
            const int count = 10;
            Duration.DecorrelatedJitterBackoff durationStrategy = new Duration.DecorrelatedJitterBackoff(count, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), false);

            // Discrete

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete();
            actualDurations.Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);

            // Continuous

            actualDurations = durationStrategy.Continuous(count).ToArray();
            actualDurations.Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_jitter_strategy_fastfirst()
        {
            const int count = 10;
            Duration.DecorrelatedJitterBackoff durationStrategy = new Duration.DecorrelatedJitterBackoff(count, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), true);

            // Discrete

            IReadOnlyList<TimeSpan> actualDurations = durationStrategy.Discrete();
            actualDurations.Take(1).Should().OnlyContain(n => n == TimeSpan.Zero);
            actualDurations.Skip(1).Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);

            // Continuous

            actualDurations = durationStrategy.Continuous(count).ToArray();
            actualDurations.Take(1).Should().OnlyContain(n => n == TimeSpan.Zero);
            actualDurations.Skip(1).Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}