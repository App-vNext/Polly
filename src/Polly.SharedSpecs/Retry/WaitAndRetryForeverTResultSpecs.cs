using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Retry
{
    [Collection("SystemClockDependantCollection")]
    public class WaitAndRetryForeverTResultSpecs : IDisposable
    {
        public WaitAndRetryForeverTResultSpecs()
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
                .WaitAndRetryForever(
                    (retryAttempt, outcome, ctx) => expectedRetryWaits[outcome.Result],
                    (_, timeSpan, __) => actualRetryWaits.Add(timeSpan)
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

        public void Dispose()
        {
            SystemClock.Reset();
        }

    }
}