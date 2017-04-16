using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class PolicyTResultSpecs
    {
        #region Execute tests

        [Fact]
        public void Executing_the_policy_function_should_execute_the_specified_function_and_return_the_result()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __) => { });

            var result = policy.Execute(() => ResultPrimitive.Good);

            result.Should()
                .Be(ResultPrimitive.Good);
        }

        #endregion

        #region ExecuteAndCapture tests

        [Fact]
        public void Executing_the_policy_function_successfully_should_return_success_result()
        {
            var result = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __) => { })
                .ExecuteAndCapture(() => ResultPrimitive.Good);

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                Result = ResultPrimitive.Good,
                FinalHandledResult = default(ResultPrimitive),
                FaultType = (FaultType?)null
            }, options => options.Excluding(o => o.Context));
        }

        [Fact]
        public void Executing_the_policy_function_and_failing_with_a_handled_result_should_return_failure_result_indicating_that_result_is_one_handled_by_this_policy()
        {
            var handledResult = ResultPrimitive.Fault;

            var result = Policy
                .HandleResult(handledResult)
                .Retry((_, __) => { })
                .ExecuteAndCapture(() => handledResult);

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                FaultType = FaultType.ResultHandledByThisPolicy,
                FinalHandledResult = handledResult,
                Result = default(ResultPrimitive)
            }, options => options.Excluding(o => o.Context));
        }

        [Fact]
        public void Executing_the_policy_function_and_returning_an_unhandled_result_should_return_result_not_indicating_any_failure()
        {
            var handledResult = ResultPrimitive.Fault;
            var unhandledResult = ResultPrimitive.Good;

            var result = Policy
                .HandleResult(handledResult)
                .Retry((_, __) => { })
                .ExecuteAndCapture(() => unhandledResult);

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                Result = unhandledResult,
                FinalHandledResult = default(ResultPrimitive),
                FaultType = (FaultType?)null
            }, options => options.Excluding(o => o.Context));
        }

        #endregion

        #region Async erroneously on sync - tests

        [Theory, MemberData(nameof(AsyncPolicies))]
        internal void Executing_the_asynchronous_policies_using_the_synchronous_execute_should_throw_an_invalid_operation_exception(Policy<ResultPrimitive> asyncPolicy, string description)
        {
            Action action = () => asyncPolicy.Execute(() => ResultPrimitive.Good);

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous-defined policies when calling the synchronous Execute (and similar) methods.");
        }

        [Theory, MemberData(nameof(AsyncPolicies))]
        internal void Executing_the_asynchronous_policies_using_the_synchronous_execute_and_capture_should_throw_an_invalid_operation_exception(Policy<ResultPrimitive> asyncPolicy, string description)
        {
            Action action = () => asyncPolicy.ExecuteAndCapture(() => ResultPrimitive.Good);

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

        private static Policy<ResultPrimitive> RetryAsyncPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __) => { });
        }

        private static Policy<ResultPrimitive> RetryForeverAsyncPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryForeverAsync((_) => { });
        }

        private static Policy<ResultPrimitive> WaitAndRetryAsyncPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .WaitAndRetryAsync(new TimeSpan[] { });
        }

        private static Policy<ResultPrimitive> WaitAndRetryForeverAsyncPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .WaitAndRetryForeverAsync(_ => new TimeSpan());
        }

        private static Policy<ResultPrimitive> CircuitBreakerAsyncPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(1, new TimeSpan());
        }

        private static Policy<ResultPrimitive> AdvancedCircuitBreakerAsyncPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .AdvancedCircuitBreakerAsync(1, TimeSpan.MaxValue, 2, new TimeSpan());
        }

        private static Policy<ResultPrimitive> TimeoutAsyncPolicy()
        {
            return Policy
                .TimeoutAsync<ResultPrimitive>(TimeSpan.MaxValue);
        }

        private static Policy<ResultPrimitive> BulkheadAsyncPolicy()
        {
            return Policy
                .BulkheadAsync<ResultPrimitive>(1);
        }

        private static Policy<ResultPrimitive> FallbackAsyncPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .FallbackAsync(ResultPrimitive.Substitute);
        }

        private static Policy<ResultPrimitive> NoOpAsyncPolicy()
        {
            return Policy.NoOpAsync<ResultPrimitive>();
        }

        #endregion

        #region Context tests
        

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => ResultPrimitive.Good, (IDictionary<string, object>)null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => ResultPrimitive.Good, (Context)null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context executionContext = new Context(executionKey);
            Context capturedContext = null;

            Policy<ResultPrimitive> policy = Policy.NoOp<ResultPrimitive>();

            policy.Execute((context) => { capturedContext = context; return ResultPrimitive.Good; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(() => ResultPrimitive.Good, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(() => ResultPrimitive.Good, (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context executionContext = new Context(executionKey);
            Context capturedContext = null;

            Policy<ResultPrimitive> policy = Policy.NoOp<ResultPrimitive>();

            policy.ExecuteAndCapture((context) => { capturedContext = context; return ResultPrimitive.Good; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context executionContext = new Context(executionKey);

            Policy<ResultPrimitive> policy = Policy.NoOp<ResultPrimitive>();

            policy.ExecuteAndCapture((context) => ResultPrimitive.Good, executionContext)
                .Context.Should().BeSameAs(executionContext);
        }

        #endregion
    }
}