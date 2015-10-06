using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

namespace Polly.Specs
{
    public class PolicySpecs
    {
        [Fact]
        public void Executing_the_policy_action_should_execute_the_specified_action()
        {
            var executed = false;

            var policy = Policy
                          .Handle<DivideByZeroException>()
                          .Retry((_, __) => { });

            policy.Execute(() => executed = true);

            executed.Should()
                    .BeTrue();
        }

        [Fact]
        public void Executing_the_policy_function_should_execute_the_specified_function_and_return_the_result()
        {
            var policy = Policy
                          .Handle<DivideByZeroException>()
                          .Retry((_, __) => { });

            var result = policy.Execute(() => 2);

            result.Should()
                  .Be(2);
        }

        [Fact]
        public void Executing_the_policy_action_successfully_should_return_success_result()
        {
            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture(() => { });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception) null,
                ExceptionType = (ExceptionType?) null,
            });
        }

        [Fact]
        public void Executing_the_policy_action_and_failing_with_a_defined_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
        {
            var definedException = new DivideByZeroException();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture(() =>
                {
                    throw definedException;
                });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = definedException,
                ExceptionType = ExceptionType.HandledByThisPolicy
            });
        }

        [Fact]
        public void Executing_the_policy_action_and_failing_with_an_undefined_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
        {
            var undefinedException = new Exception();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture(() =>
                {
                    throw undefinedException;
                });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = undefinedException,
                ExceptionType = ExceptionType.Unhandled
            });
        }

        [Fact]
        public void Executing_the_policy_function_successfully_should_return_success_result()
        {
            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture(() => Int32.MaxValue);

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                Result = Int32.MaxValue
            });
        }

        [Fact]
        public void Executing_the_policy_function_and_failing_with_a_defined_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
        {
            var definedException = new DivideByZeroException();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture<int>(() =>
                {
                    throw definedException;
                });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = definedException,
                ExceptionType = ExceptionType.HandledByThisPolicy,
                Result = default(int)
            });
        }

        [Fact]
        public void Executing_the_policy_function_and_failing_with_an_undefined_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
        {
            var undefinedException = new Exception();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture<int>(() =>
                {
                    throw undefinedException;
                });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = undefinedException,
                ExceptionType = ExceptionType.Unhandled,
                Result = default(int)
            });
        }

        [Theory, MemberData("AsyncPolicies")]
        public void Executing_the_asynchronous_policies_using_the_synchronous_execute_should_throw_an_invalid_operation_exception(Policy asyncPolicy, string description)
        {
            Action action = () => asyncPolicy.Execute(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");
        }

        [Theory, MemberData("AsyncPolicies")]
        public void Executing_the_asynchronous_policies_using_the_synchronous_execute_and_capture_should_throw_an_invalid_operation_exception(Policy asyncPolicy, string description)
        {
            Action action = () => asyncPolicy.ExecuteAndCapture(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");
        }

        public static IEnumerable<object[]> AsyncPolicies
        {
            get
            {
                return new[]
                {
                    new object[] {RetryAsyncPolicy(), "retry"},
                    new object[] {RetryForeverAsyncPolicy(), "retry forever"},
                    new object[] {WaitAndRetryAsyncPolicy(), "wait and retry"},
                    new object[] {CircuitBreakerAsyncPolicy(), "circuit breaker"}
                };
            }
        }

        private static Policy RetryAsyncPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { });
        }

        private static Policy RetryForeverAsyncPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync((_) => { });
        }

        private static Policy WaitAndRetryAsyncPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryAsync(new TimeSpan[] {});
        }

        private static Policy CircuitBreakerAsyncPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(1, new TimeSpan());
        }
    }
}