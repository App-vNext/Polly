using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.Utilities;
using Xunit;

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
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
            }, options => options.Excluding(o => o.Context));
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
            }, options => options.Excluding(o => o.Context));
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
            }, options => options.Excluding(o => o.Context));
        }

        [Fact]
        public void Executing_the_policy_function_successfully_should_return_success_result()
        {
            var result = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { })
                .ExecuteAndCapture(() => int.MaxValue);

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                FaultType = (FaultType?)null,
                FinalHandledResult = default(int),
                Result = int.MaxValue
            }, options => options.Excluding(o => o.Context));
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
            }, options => options.Excluding(o => o.Context));
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
            }, options => options.Excluding(o => o.Context));
        }

        #endregion

        #region Async erroneously on sync - tests

        [Theory, MemberData(nameof(AsyncPolicies))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public void Executing_the_asynchronous_policies_using_the_synchronous_execute_should_throw_an_invalid_operation_exception(Policy asyncPolicy, string description)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        {
            Action action = () => asyncPolicy.Execute(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
        }

        [Theory, MemberData(nameof(AsyncPolicies))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public void Executing_the_asynchronous_policies_using_the_synchronous_execute_and_capture_should_throw_an_invalid_operation_exception(Policy asyncPolicy, string description)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        {
            Action action = () => asyncPolicy.ExecuteAndCapture(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
        }

        public static IEnumerable<object[]> AsyncPolicies => new[]
        {
            new object[] {RetryAsyncPolicy(), "retry"},
            new object[] {RetryForeverAsyncPolicy(), "retry forever"},
            new object[] {WaitAndRetryAsyncPolicy(), "wait and retry"},
            new object[] {WaitAndRetryForeverAsyncPolicy(), "wait and retry forever"},
            new object[] {CircuitBreakerAsyncPolicy(), "circuit breaker"},
            new object[] {AdvancedCircuitBreakerAsyncPolicy(), "advanced circuit breaker"},
            new object[] {TimeoutAsyncPolicy(), "timeout"},
            new object[] {BulkheadAsyncPolicy(), "bulkhead"},
            new object[] {FallbackAsyncPolicy(), "fallback"},
            new object[] {NoOpAsyncPolicy(), "no-op"}
        };

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
                .WaitAndRetryAsync(new TimeSpan[] { });
        }

        private static Policy WaitAndRetryForeverAsyncPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(_ => new TimeSpan());
        }

        private static Policy CircuitBreakerAsyncPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(1, new TimeSpan());
        }

        private static Policy AdvancedCircuitBreakerAsyncPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreakerAsync(1, TimeSpan.MaxValue, 2, new TimeSpan());
        }

        private static Policy TimeoutAsyncPolicy() => Policy
                .TimeoutAsync(TimeSpan.MaxValue);

        private static Policy BulkheadAsyncPolicy() => Policy
                .BulkheadAsync(1);

        private static Policy FallbackAsyncPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .FallbackAsync(_ => TaskHelper.EmptyTask);
        }

        private static Policy NoOpAsyncPolicy() => Policy.NoOpAsync();

        #endregion

        #region Context tests

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(ctx => { }, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(ctx => { }, null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(ctx => 2, (IDictionary<string, object>)null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(ctx => 2, null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            var operationKey = "SomeKey";
            var executionContext = new Context(operationKey);
            Context capturedContext = null;

            Policy policy = Policy.NoOp();

            policy.Execute((context) => { capturedContext = context; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(ctx => { }, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(ctx => { }, null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(ctx => 2, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(ctx => 2, null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            var operationKey = "SomeKey";
            var executionContext = new Context(operationKey);
            Context capturedContext = null;

            Policy policy = Policy.NoOp();

            policy.ExecuteAndCapture((context) => { capturedContext = context; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
        {
            var operationKey = "SomeKey";
            var executionContext = new Context(operationKey);

            Policy policy = Policy.NoOp();

            policy.ExecuteAndCapture((context) => { }, executionContext)
                .Context.Should().BeSameAs(executionContext);
        }

        #endregion
    }
}