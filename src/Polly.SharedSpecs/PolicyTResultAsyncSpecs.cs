using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CSharp.RuntimeBinder;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs
{
    public class PolicyTResultAsyncSpecs
    {
        #region Execute tests

        [Fact]
        public async Task Executing_the_policy_function_should_execute_the_specified_function_and_return_the_result()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __) => { });

            var result = await policy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good));

            result.Should()
                .Be(ResultPrimitive.Good);
        }

        #endregion

        #region ExecuteAndCapture tests

        [Fact]
        public async Task Executing_the_policy_function_successfully_should_return_success_result()
        {
            var result = await Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __) => { })
                .ExecuteAndCaptureAsync(() => Task.FromResult(ResultPrimitive.Good));

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
        public async Task Executing_the_policy_function_and_failing_with_a_handled_result_should_return_failure_result_indicating_that_result_is_one_handled_by_this_policy()
        {
            var handledResult = ResultPrimitive.Fault;

            var result = await Policy
                .HandleResult(handledResult)
                .RetryAsync((_, __) => { })
                .ExecuteAndCaptureAsync(() => Task.FromResult(handledResult));

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
        public async Task Executing_the_policy_function_and_returning_an_unhandled_result_should_return_result_not_indicating_any_failure()
        {
            var handledResult = ResultPrimitive.Fault;
            var unhandledResult = ResultPrimitive.Good;

            var result = await Policy
                .HandleResult(handledResult)
                .RetryAsync((_, __) => { })
                .ExecuteAndCaptureAsync(() => Task.FromResult(unhandledResult));

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

        #region Sync erroneously on async - tests

        [Theory, MemberData(nameof(SyncPolicies))]
        internal void Executing_the_synchronous_policies_using_the_asynchronous_execute_should_throw_an_invalid_operation_exception(Policy<ResultPrimitive> syncPolicy, string description)
        {
            syncPolicy
                .Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.");
        }

        [Theory, MemberData(nameof(SyncPolicies))]
        internal void Executing_the_synchronous_policies_using_the_asynchronous_execute_and_capture_should_throw_an_invalid_operation_exception(Policy<ResultPrimitive> syncPolicy, string description)
        {
            syncPolicy
                .Awaiting(async x => await x.ExecuteAndCaptureAsync(() => Task.FromResult(ResultPrimitive.Good)))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.");
        }

        public static IEnumerable<object[]> SyncPolicies => new[]
        {
            new object[] {RetryPolicy(), "retry"},
            new object[] {RetryForeverPolicy(), "retry forever"},
            new object[] {WaitAndRetryPolicy(), "wait and retry"},
            new object[] {WaitAndRetryForeverPolicy(), "wait and retry"},
            new object[] {CircuitBreakerPolicy(), "circuit breaker"},
            new object[] {AdvancedCircuitBreakerPolicy(), "advanced circuit breaker"},
            new object[] {TimeoutPolicy(), "timeout"},
            new object[] {BulkheadPolicy(), "bulkhead"},
            new object[] {FallbackPolicy(), "fallback"},
            new object[] {NoOpPolicy(), "no-op"}
        };

        private static Policy<ResultPrimitive> RetryPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .Retry((_, __) => { });
        }

        private static Policy<ResultPrimitive> RetryForeverPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryForever((_) => { });
        }

        private static Policy<ResultPrimitive> WaitAndRetryPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .WaitAndRetry(new TimeSpan[] { });
        }

        private static Policy<ResultPrimitive> WaitAndRetryForeverPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .WaitAndRetryForever(_ => new TimeSpan());
        }

        private static Policy<ResultPrimitive> CircuitBreakerPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(1, new TimeSpan());
        }

        private static Policy<ResultPrimitive> AdvancedCircuitBreakerPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .AdvancedCircuitBreaker(1, TimeSpan.MaxValue, 2, new TimeSpan());
        }

        private static Policy<ResultPrimitive> TimeoutPolicy()
        {
            return Policy
                .Timeout<ResultPrimitive>(TimeSpan.MaxValue);
        }

        private static Policy<ResultPrimitive> BulkheadPolicy()
        {
            return Policy
                .Bulkhead<ResultPrimitive>(1);
        }

        private static Policy<ResultPrimitive> FallbackPolicy()
        {
            return Policy
                .HandleResult(ResultPrimitive.Fault)
                .Fallback(ResultPrimitive.Substitute);
        }

        private static Policy<ResultPrimitive> NoOpPolicy()
        {
            return Policy.NoOp<ResultPrimitive>();
        }

        #endregion

        #region Context tests


        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good), (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good), (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(() => Task.FromResult(ResultPrimitive.Good), (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public async Task Executing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context executionContext = new Context(executionKey);
            Context capturedContext = null;

            Policy<ResultPrimitive> policy = Policy.NoOpAsync<ResultPrimitive>();

            await policy.ExecuteAsync((context) => { capturedContext = context; return Task.FromResult(ResultPrimitive.Good); }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(() => Task.FromResult(ResultPrimitive.Good), (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public async Task Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context executionContext = new Context(executionKey);
            Context capturedContext = null;

            Policy<ResultPrimitive> policy = Policy.NoOpAsync<ResultPrimitive>();

            await policy.ExecuteAndCaptureAsync((context) => { capturedContext = context; return Task.FromResult(ResultPrimitive.Good); }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public async Task Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context executionContext = new Context(executionKey);

            Policy<ResultPrimitive> policy = Policy.NoOpAsync<ResultPrimitive>();

            (await policy.ExecuteAndCaptureAsync((context) => Task.FromResult(ResultPrimitive.Good), executionContext))
                .Context.Should().BeSameAs(executionContext);
        }

        #endregion
    }
}