using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Retry
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class WaitAndRetryForeverTResultAsyncSpecs : IDisposable
    {
        public WaitAndRetryForeverTResultAsyncSpecs()
        {
            // do nothing on call to sleep
            SystemClock.SleepAsync = (_, __) => Task.CompletedTask;
        }

        [Fact]
        public async Task Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
        {
            var expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>(){

                {ResultPrimitive.Fault, 2.Seconds()},
                {ResultPrimitive.FaultAgain, 4.Seconds()},
            };

            var actualRetryWaits = new List<TimeSpan>();

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .WaitAndRetryForeverAsync(
                    (retryAttempt, outcome, ctx) => expectedRetryWaits[outcome.Result],
                    (_, timeSpan, __) =>
                    {
                        actualRetryWaits.Add(timeSpan);
                        return Task.CompletedTask;
                    });

            using (var enumerator = expectedRetryWaits.GetEnumerator())
            {
                await policy.ExecuteAsync(async () =>
                {
                    await Task.CompletedTask;
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current.Key;
                    }
                    else
                    {
                        return ResultPrimitive.Undefined;
                    }
                });
            }

            actualRetryWaits.Should().ContainInOrder(expectedRetryWaits.Values);
        }

        public void Dispose() => SystemClock.Reset();

    }
}