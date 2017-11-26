using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Retry
{
    [Collection("SystemClockDependantCollection")]
    public class WaitAndRetryForeverSpecs : IDisposable
    {
        public WaitAndRetryForeverSpecs()
        {
            // do nothing on call to sleep
            SystemClock.Sleep = (_, __) => { };
        }

        [Fact]
        public void Should_throw_when_sleep_duration_provider_is_null_without_context()
        {
            Action<Exception, TimeSpan> onRetry = (_, __) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForever(null, onRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("sleepDurationProvider");
        }

        [Fact]
        public void Should_throw_when_sleep_duration_provider_is_null_with_context()
        {
            Action<Exception, TimeSpan, Context> onRetry = (_, __, ___) => { };

            Func<int, Context, TimeSpan> sleepDurationProvider = null;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForever(sleepDurationProvider, onRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("sleepDurationProvider");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_without_context()
        {
            Action<Exception, TimeSpan> nullOnRetry = null;
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForever(provider, nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_with_context()
        {
            Action<Exception, TimeSpan, Context> nullOnRetry = null;
            Func<int, Context, TimeSpan> provider = (i, ctx) => TimeSpan.Zero;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForever(provider, nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForever(_ => new TimeSpan());

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetryForever(_ => new TimeSpan());

            policy.Invoking(x => x.RaiseException<ArgumentException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
        {
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;
            
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForever(provider);

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        } 

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
        {
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetryForever(provider);

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }
       
        [Fact]
        public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
        {
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .WaitAndRetryForever(provider);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
        {
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Or<ArgumentException>(e => false)
                .WaitAndRetryForever(provider);

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();

            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .WaitAndRetryForever(provider);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();
        }
        
        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();

            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Or<ArgumentException>(e => true)
               .WaitAndRetryForever(provider);

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_sleep_if_no_retries()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();

            var totalTimeSlept = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForever(provider);

            SystemClock.Sleep = (span, ct) => totalTimeSlept += span.Seconds;

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();

            totalTimeSlept.Should()
                          .Be(0);
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_exception()
        {
            var expectedExceptions = new object[] { "Exception #1", "Exception #2", "Exception #3" };
            var retryExceptions = new List<Exception>();
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForever(provider, (exception, _) => retryExceptions.Add(exception));

            policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

            retryExceptions
                .Select(x => x.HelpLink)
                .Should()
                .ContainInOrder(expectedExceptions);
        }

        [Fact]
        public void Should_not_call_onretry_when_no_retries_are_performed()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForever(provider, (exception, _) => retryExceptions.Add(exception));

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldThrow<ArgumentException>();

            retryExceptions.Should().BeEmpty();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_policy()
        {
            Func<int, Context, TimeSpan> provider = (i, ctx) => 1.Seconds();

            string contextValue = null;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForever(
                provider, 
                (_, __, context) => contextValue = context["key"].ToString());

            policy.RaiseException<DivideByZeroException>(
                new { key = "original_value" }.AsDictionary()
            );

            contextValue.Should().Be("original_value");

            policy.RaiseException<DivideByZeroException>(
                new { key = "new_value" }.AsDictionary()
            );

            contextValue.Should().Be("new_value");
        }

        [Fact]
        public void Should_calculate_retry_timespans_from_current_retry_attempt_and_timespan_provider()
        {
            var expectedRetryWaits = new[]
                {
                    2.Seconds(),
                    4.Seconds(),
                    8.Seconds(),
                    16.Seconds(),
                    32.Seconds()
                };

            var actualRetryWaits = new List<TimeSpan>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForever(
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (_, timeSpan) => actualRetryWaits.Add(timeSpan)
                );

            policy.RaiseException<DivideByZeroException>(5);

            actualRetryWaits.Should()
                       .ContainInOrder(expectedRetryWaits);
        }

        [Fact]
        public void Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
        {
            Dictionary<Exception, TimeSpan> expectedRetryWaits = new Dictionary<Exception, TimeSpan>(){

                {new DivideByZeroException(), 2.Seconds()},
                {new ArgumentNullException(), 4.Seconds()},
            };

            var actualRetryWaits = new List<TimeSpan>();

            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryForever(
                    (retryAttempt, exc, ctx) => expectedRetryWaits[exc],
                    (_, timeSpan, __) => actualRetryWaits.Add(timeSpan)
                );

            using (var enumerator = expectedRetryWaits.GetEnumerator())
            {
                policy.Execute(() => { if (enumerator.MoveNext()) throw enumerator.Current.Key; });
            }

            actualRetryWaits.Should().ContainInOrder(expectedRetryWaits.Values);
        }

        [Fact]
        public void Should_be_able_to_pass_retry_duration_from_execution_to_sleepDurationProvider_via_context()
        {
            var expectedRetryDuration = 1.Seconds();
            TimeSpan? actualRetryDuration = null;

            TimeSpan defaultRetryAfter = 30.Seconds();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForever(
                    sleepDurationProvider: (retryAttempt, context) => context.ContainsKey("RetryAfter") ? (TimeSpan) context["RetryAfter"] : defaultRetryAfter, // Set sleep duration from Context, when available.
                    onRetry: (_, timeSpan, __) => actualRetryDuration = timeSpan // Capture the actual sleep duration that was used, for test verification purposes.
                );

            bool failedOnce = false;
            policy.Execute(context =>
                {
                    // Run some remote call; maybe it returns a RetryAfter header, which we can pass back to the sleepDurationProvider, via the context.
                    context["RetryAfter"] = expectedRetryDuration;

                    if (!failedOnce)
                    {
                        failedOnce = true;
                        throw new DivideByZeroException();
                    }
                }, 
                new {RetryAfter = defaultRetryAfter}.AsDictionary() // Can also set an initial value for RetryAfter, in the Context passed into the call.
                );

            actualRetryDuration.Should().Be(expectedRetryDuration);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}