﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;
using FluentAssertions.Extensions;

namespace Polly.Specs.Retry
{
    [Collection(Constants.SystemClockDependentTestCollection)]
    public class WaitAndRetryForeverTResultAsyncSpecs : IDisposable
    {
        public WaitAndRetryForeverTResultAsyncSpecs()
        {
            // do nothing on call to sleep
            SystemClock.SleepAsync = (_, __) => TaskHelper.EmptyTask;
        }

        [Fact]
        public async Task Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
        {
            Dictionary<ResultPrimitive, TimeSpan> expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>(){

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
                        return TaskHelper.EmptyTask;
                    });

            using (var enumerator = expectedRetryWaits.GetEnumerator())
            {
                await policy.ExecuteAsync(async () =>
                {
                    await TaskHelper.EmptyTask;
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