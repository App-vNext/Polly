using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.SharedSpecs
{
    public class WaitAndRetryForeverAsyncSpecs
    {
        public WaitAndRetryForeverAsyncSpecs()
        {
            // do nothing on call to sleep
            SystemClock.SleepAsync = (_, __) => Task.FromResult(0);
        }

        [Fact]
        public void Should_throw_when_sleep_duration_provider_is_null_without_context()
        {
            Action<Exception, TimeSpan> onRetry = (_, __) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForeverAsync(null, onRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("sleepDurationProvider");
        }

        [Fact]
        public void Should_throw_when_sleep_duration_provider_is_null_with_context()
        {
            Action<Exception, TimeSpan, Context> onRetry = (_, __, ___) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForeverAsync(null, onRetry);

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
                                      .WaitAndRetryForeverAsync(provider, nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_with_context()
        {
            Action<Exception, TimeSpan, Context> nullOnRetry = null;
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForeverAsync(provider, nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(_ => new TimeSpan());

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetryForeverAsync(_ => new TimeSpan());

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
        {
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
        {
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
        {
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
        {
            Func<int, TimeSpan> provider = i => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Or<ArgumentException>(e => false)
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();

            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();

            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Or<ArgumentException>(e => true)
               .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_sleep_if_no_retries()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();

            var totalTimeSlept = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            SystemClock.Sleep = span => totalTimeSlept += span.Seconds;

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();

            totalTimeSlept.Should()
                          .Be(0);
        }

        [Fact]
        public void Should_not_call_onretry_when_no_retries_are_performed()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();
            var retryCounts = new List<int>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldThrow<ArgumentException>();

            retryCounts.Should()
                       .BeEmpty();
        }

        [Fact]
        public void Should_create_new_state_for_each_call_to_policy()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldNotThrow();

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_policy()
        {
            Func<int, TimeSpan> provider = i => 1.Seconds();

            string contextValue = null;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(
                provider,
                (_, __, context) => contextValue = context["key"].ToString());

            policy.RaiseExceptionAsync<DivideByZeroException>(
                new { key = "original_value" }.AsDictionary()
            );

            contextValue.Should().Be("original_value");

            policy.RaiseExceptionAsync<DivideByZeroException>(
                new { key = "new_value" }.AsDictionary()
            );

            contextValue.Should().Be("new_value");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_without_context_when_using_provider_overload()
        {
            Action<Exception, TimeSpan> nullOnRetry = null;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForeverAsync(_ => new TimeSpan(), nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_with_context_when_using_provider_overload()
        {
            Action<Exception, TimeSpan, Context> nullOnRetry = null;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForeverAsync(_ => new TimeSpan(), nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_calculate_retry_timespans_from_current_retry_attempt_and_timespan_provider()
        {
            var expectedRetryCounts = new[]
                {
                    2.Seconds(),
                    4.Seconds(),
                    8.Seconds(),
                    16.Seconds(),
                    32.Seconds()
                };

            var retryTimeSpans = new List<TimeSpan>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (_, timeSpan) => retryTimeSpans.Add(timeSpan)
                );

            policy.RaiseExceptionAsync<DivideByZeroException>(5);

            retryTimeSpans.Should()
                       .ContainInOrder(expectedRetryCounts);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
