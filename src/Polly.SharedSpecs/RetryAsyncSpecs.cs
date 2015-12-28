using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class RetryAsyncSpecs
    {
        [Fact]
        public void Should_throw_when_retry_count_is_less_than_zero_without_context()
        {
            Action<Exception, int> onRetry = (_, __) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .RetryAsync(-1, onRetry);
        
            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                  .ParamName.Should().Be("retryCount");
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync(3);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .RetryAsync(3);

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_thrown_less_number_of_times_than_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync(3);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_of_times_than_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .RetryAsync(3);

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_specified_exception_thrown_more_times_then_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync(3);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(3 + 1))
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_then_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .RetryAsync(3);

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>(3 + 1))
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync();

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .RetryAsync();

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .RetryAsync();

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Or<ArgumentException>(e => false)
                .RetryAsync();

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .RetryAsync();

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Or<ArgumentException>(e => true)
                .RetryAsync();

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
        {
            var expectedRetryCounts = new[] { 1, 2, 3 };
            var retryCounts = new List<int>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync(3, (_, retyCount) => retryCounts.Add(retyCount));

            policy.RaiseExceptionAsync<DivideByZeroException>(3);

            retryCounts.Should()
                       .ContainInOrder(expectedRetryCounts);
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_exception()
        {
            var expectedExceptions = new object[] { "Exception #1", "Exception #2", "Exception #3" };
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync(3, (exception, _) => retryExceptions.Add(exception));

            policy.RaiseExceptionAsync<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

            retryExceptions
                .Select(x => x.HelpLink)
                .Should()
                .ContainInOrder(expectedExceptions);
        }

        [Fact]
        public void Should_not_call_onretry_when_no_retries_are_performed()
        {
            var retryCounts = new List<int>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, retyCount) => retryCounts.Add(retyCount));

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldThrow<ArgumentException>();

            retryCounts.Should()
                       .BeEmpty();
        }

        [Fact]
        public void Should_create_new_state_for_each_call_to_policy()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync();

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldNotThrow();

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_call_onretry_when_retry_count_is_zero()
        {
            bool retryInvoked = false;

            Action<Exception, int> onRetry = (_, __) => { retryInvoked = true; };

            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync(0, onRetry);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            retryInvoked.Should().BeFalse();
        }
    }
}