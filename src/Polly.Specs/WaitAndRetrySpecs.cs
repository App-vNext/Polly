using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs
{
    public class WaitAndRetrySpecs : IDisposable
    {
        public WaitAndRetrySpecs()
        {
            // do nothing on call to sleep
            SystemClock.Sleep = (_) => { };
        }

        [Fact]
        public void Should_throw_when_sleep_durations_is_null_without_context()
        {
            Action<Exception, TimeSpan> onRetry = (_, __) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(null, onRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("sleepDurations");
        }

        [Fact]
        public void Should_throw_when_sleep_durations_is_null_with_context()
        {
            Action<Exception, TimeSpan, Context> onRetry = (_, __, ___) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(null, onRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("sleepDurations");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_without_context()
        {
            Action<Exception, TimeSpan> nullOnRetry = null;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(Enumerable.Empty<TimeSpan>(), nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_with_context()
        {
            Action<Exception, TimeSpan, Context> nullOnRetry = null;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(Enumerable.Empty<TimeSpan>(), nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }
        
        [Fact]
        public void Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_there_are_sleep_durations()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_there_are_sleep_durations()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            policy.Invoking(x => x.RaiseException<ArgumentException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_thrown_less_number_of_times_then_there_are_sleep_durations()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(2))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_then_times_as_there_are_sleep_durations()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            policy.Invoking(x => x.RaiseException<ArgumentException>(2))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_specified_exception_thrown_more_times_then_there_are_sleep_durations()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(3 + 1))
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_there_are_sleep_durations()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            policy.Invoking(x => x.RaiseException<ArgumentException>(3 + 1))
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }
 [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type_async()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }
  [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types_async()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Or<ArgumentException>(e => false)
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .WaitAndRetry(new[]
                {
                   1.Seconds()
                });

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied_async()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .WaitAndRetryAsync(new[]
                {
                   1.Seconds()
                });

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Or<ArgumentException>(e => true)
               .WaitAndRetry(new[]
                {
                   1.Seconds()
                });

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied_async()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Or<ArgumentException>(e => true)
               .WaitAndRetryAsync(new[]
                {
                   1.Seconds()
                });

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_same_number_of_times_as_there_are_sleep_durations()
        {
            var totalTimeSlept = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            SystemClock.Sleep = span => totalTimeSlept += span.Seconds;

            policy.RaiseException<DivideByZeroException>(3);

            totalTimeSlept.Should()
                          .Be(1 + 2 + 3);
        }

        [Fact]
        public async Task Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_same_number_of_times_as_there_are_sleep_durations_async()
        {
            var totalTimeSlept = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryAsync(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            SystemClock.Sleep = span => totalTimeSlept += span.Seconds;

            await policy.RaiseExceptionAsync<DivideByZeroException>(3);

            totalTimeSlept.Should()
                          .Be(1 + 2 + 3);
        }

        [Fact]
        public void Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_more_number_of_times_as_there_are_sleep_durations()
        {
            var totalTimeSlept = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            SystemClock.Sleep = span => totalTimeSlept += span.Seconds;

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(3 + 1))
                  .ShouldThrow<DivideByZeroException>();

            totalTimeSlept.Should()
                          .Be(1 + 2 + 3);
        }

        [Fact]
        public void Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_less_number_of_times_then_there_are_sleep_durations()
        {
            var totalTimeSlept = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                });

            SystemClock.Sleep = span => totalTimeSlept += span.Seconds;

            policy.RaiseException<DivideByZeroException>(2);

            totalTimeSlept.Should()
                          .Be(1 + 2);
        }

        [Fact]
        public void Should_not_sleep_if_no_retries()
        {
            var totalTimeSlept = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            SystemClock.Sleep = span => totalTimeSlept += span.Seconds;

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();

            totalTimeSlept.Should()
                          .Be(0);
        }

        [Fact]
        public void Should_not_sleep_if_no_retries_async()
        {
            var totalTimeSlept = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            SystemClock.Sleep = span => totalTimeSlept += span.Seconds;

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();

            totalTimeSlept.Should()
                          .Be(0);
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_timespan()
        {
            var expectedRetryCounts = new []
                {
                    1.Seconds(), 
                    2.Seconds(), 
                    3.Seconds()
                };

            var retryTimeSpans = new List<TimeSpan>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                }, (_, timeSpan) => retryTimeSpans.Add(timeSpan));

            policy.RaiseException<DivideByZeroException>(3);

            retryTimeSpans.Should()
                       .ContainInOrder(expectedRetryCounts);
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_exception()
        {
            var expectedExceptions = new object[] { "Exception #1", "Exception #2", "Exception #3" };
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                }, (exception, _) => retryExceptions.Add(exception));

            policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

            retryExceptions
                .Select(x => x.HelpLink)
                .Should()
                .ContainInOrder(expectedExceptions);
        }

        [Fact]
        public void Should_call_onretry_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                   1.Seconds(),
                   2.Seconds(),
                   3.Seconds()
                }, (_, __, context) => contextData = context);

            policy.RaiseException<DivideByZeroException>(
                new { key1 = "value1", key2 = "value2" }.AsDictionary()
            );

            contextData.Should()
                       .ContainKeys("key1", "key2").And
                       .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_not_call_onretry_when_no_retries_are_performed()
        {
            var retryCounts = new List<int>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldThrow<ArgumentException>();

            retryCounts.Should()
                       .BeEmpty();
        }

        [Fact]
        public void Should_create_new_state_for_each_call_to_policy()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                    1.Seconds()
                });

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_policy()
        {
            string contextValue = null;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                {
                    1.Seconds()
                }, 
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
        public void Should_throw_when_retry_count_is_less_than_one_without_context()
        {
            Action<Exception, TimeSpan> onRetry = (_, __) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(0, _ => new TimeSpan(), onRetry);
                                           
            policy.ShouldThrow<ArgumentOutOfRangeException>().And                  
                  .ParamName.Should().Be("retryCount");
        }

        [Fact]
        public void Should_throw_when_retry_count_is_less_than_one_with_context()
        {
            Action<Exception, TimeSpan, Context> onRetry = (_, __, ___) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(0, _ => new TimeSpan(), onRetry);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                  .ParamName.Should().Be("retryCount");
        }

        [Fact]
        public void Should_throw_when_sleep_duration_provider_is_null_without_context()
        {
            Action<Exception, TimeSpan> onRetry = (_, __) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(1, null, onRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("sleepDurationProvider");
        }

        [Fact]
        public void Should_throw_when_sleep_duration_provider_is_null_with_context()
        {
            Action<Exception, TimeSpan, Context> onRetry = (_, __, ___) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(1, null, onRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("sleepDurationProvider");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_without_context_when_using_provider_overload()
        {
            Action<Exception, TimeSpan> nullOnRetry = null;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(1, _ => new TimeSpan(), nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_with_context_when_using_provider_overload()
        {
            Action<Exception, TimeSpan, Context> nullOnRetry = null;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetry(1, _ => new TimeSpan(), nullOnRetry);

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
                .WaitAndRetry(5, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                    (_, timeSpan) => retryTimeSpans.Add(timeSpan)
                );

            policy.RaiseException<DivideByZeroException>(5);

            retryTimeSpans.Should()
                       .ContainInOrder(expectedRetryCounts);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}