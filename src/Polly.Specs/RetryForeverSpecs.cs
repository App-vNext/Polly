using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class RetryForeverSpecs
    {
        [Fact]
        public void Should_throw_when_on_retry_action_is_null()
        {
            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .RetryForever(null);

            policy.ShouldThrow<ArgumentNullException>()
                  .And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForever();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .RetryForever();

            policy.Invoking(x => x.RaiseException<ArgumentException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForever();

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .RetryForever();

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .RetryForever();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Or<ArgumentException>(e => false)
                .RetryForever();

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .RetryForever();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Or<ArgumentException>(e => true)
                .RetryForever();

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_exception()
        {
            var expectedExceptions = new object[] {"Exception #1", "Exception #2", "Exception #3"};
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForever(exception => retryExceptions.Add(exception));

            policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

            retryExceptions
                .Select(x => x.HelpLink)
                .Should()
                .ContainInOrder(expectedExceptions);
        }

        [Fact]
        public void Should_not_call_onretry_when_no_retries_are_performed()
        {
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForever(exception => retryExceptions.Add(exception));

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldThrow<ArgumentException>();

            retryExceptions.Should()
                           .BeEmpty();
        }
    }
}