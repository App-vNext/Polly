using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class RetrySpecs
    {
        [Fact]
        public void Should_throw_when_retry_count_is_less_than_one()
        {
            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .Retry(0);

            policy.ShouldThrow<ArgumentOutOfRangeException>()
                  .And
                  .ParamName.Should()
                  .Be("retryCount");
        }

        [Fact]
        public void Should_throw_when_on_retry_action_is_null()
        {
            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .Retry(1, null);

            policy.ShouldThrow<ArgumentNullException>()
                  .And
                  .ParamName.Should()
                  .Be("onRetry");
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<ArgumentException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_thrown_less_number_of_times_than_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_of_times_than_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_specified_exception_thrown_more_times_then_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(3 + 1))
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_then_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<ArgumentException>(3 + 1))
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry();

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .Retry();

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Retry();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Or<ArgumentException>(e => false)
                .Retry();

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Retry();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Or<ArgumentException>(e => true)
                .Retry();

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
        {
            var expectedRetryCounts = new[] {1, 2, 3};
            var retryCounts = new List<int>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3, (_, retyCount) => retryCounts.Add(retyCount));

            policy.RaiseException<DivideByZeroException>(3);

            retryCounts.Should()
                       .ContainInOrder(expectedRetryCounts);
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_exception()
        {
            var expectedExceptions = new object[] {"Exception #1", "Exception #2", "Exception #3"};
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3, (exception, _) => retryExceptions.Add(exception));

            policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

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
                .Retry(1, (_, retyCount) => retryCounts.Add(retyCount));

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
                .Retry();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();
        }
    }
}