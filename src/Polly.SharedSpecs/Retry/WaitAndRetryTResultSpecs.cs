using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
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

            var actualRetryWaits = new List<TimeSpan>();

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .WaitAndRetry(2,
                    (retryAttempt, outcome, ctx) => expectedRetryWaits[outcome.Result],
                    (_, timeSpan, __, ___) => actualRetryWaits.Add(timeSpan)
                );

            using (var enumerator = expectedRetryWaits.GetEnumerator())
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
            var durationStrategy = new Duration.LinearBackoff(5, TimeSpan.FromSeconds(1), 2);
            var actualDurations = durationStrategy.Generate();

            var expectedDurations = new TimeSpan[5]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(7),
                TimeSpan.FromSeconds(9)
            };

            actualDurations.Should().ContainInOrder(expectedDurations);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_exponential_strategy()
        {
            var durationStrategy = new Duration.ExponentialBackoff(5, TimeSpan.FromSeconds(1));
            var actualDurations = durationStrategy.Generate();

            var expectedDurations = new TimeSpan[5]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(16)
            };

            actualDurations.Should().ContainInOrder(expectedDurations);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_from_jitter_strategy()
        {
            var durationStrategy = new Duration.DecorrelatedJitterBackoff(10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
            var actualDurations = durationStrategy.Generate();

            actualDurations.Should().OnlyContain(n => n >= durationStrategy.MinDelay && n <= durationStrategy.MaxDelay);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}