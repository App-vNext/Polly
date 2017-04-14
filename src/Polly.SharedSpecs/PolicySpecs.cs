using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

namespace Polly.Specs
{
    public class PolicySpecs
    {
        #region Execute tests

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

        #endregion

        #region ExecuteAndCapture tests

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
        public void Executing_the_policy_action_and_failing_with_a_handled_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
        {
            var handledException = new DivideByZeroException();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture(() =>
                {
                    throw handledException;
                });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = handledException,
                ExceptionType = ExceptionType.HandledByThisPolicy
            });
        }

        [Fact]
        public void Executing_the_policy_action_and_failing_with_an_unhandled_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
        {
            var unhandledException = new Exception();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture(() =>
                {
                    throw unhandledException;
                });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = unhandledException,
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
                FaultType = (FaultType?)null,
                FinalHandledResult = default(int),
                Result = Int32.MaxValue
            });
        }

        [Fact]
        public void Executing_the_policy_function_and_failing_with_a_handled_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
        {
            var handledException = new DivideByZeroException();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture<int>(() =>
                {
                    throw handledException;
                });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = handledException,
                ExceptionType = ExceptionType.HandledByThisPolicy,
                FaultType = FaultType.ExceptionHandledByThisPolicy,
                FinalHandledResult = default(int),
                Result = default(int)
            });
        }

        [Fact]
        public void Executing_the_policy_function_and_failing_with_an_unhandled_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
        {
            var unhandledException = new Exception();

            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture<int>(() =>
                {
                    throw unhandledException;
                });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = unhandledException,
                ExceptionType = ExceptionType.Unhandled,
                FaultType = FaultType.UnhandledException,
                FinalHandledResult = default(int),
                Result = default(int)
            });
        }

        #endregion

        #region Async erroneously on sync - tests

        [Theory, MemberData("AsyncPolicies")]
        public void Executing_the_asynchronous_policies_using_the_synchronous_execute_should_throw_an_invalid_operation_exception(Policy asyncPolicy, string description)
        {
            Action action = () => asyncPolicy.Execute(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
        }

        [Theory, MemberData("AsyncPolicies")]
        public void Executing_the_asynchronous_policies_using_the_synchronous_execute_and_capture_should_throw_an_invalid_operation_exception(Policy asyncPolicy, string description)
        {
            Action action = () => asyncPolicy.ExecuteAndCapture(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
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

        #endregion

        #region Context tests

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => { }, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => { }, (Context)null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => 2, (IDictionary<string, object>)null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => 2, (Context)null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(() => { }, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(() => { }, (Context)null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(() => 2, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(() => 2, (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        #endregion
    }
}