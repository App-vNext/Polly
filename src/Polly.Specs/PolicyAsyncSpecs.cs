﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class PolicyAsyncSpecs
    {
        #region Execute tests

        [Fact]
        public async Task Executing_the_policy_action_should_execute_the_specified_async_action()
        {
            var executed = false;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { });

            await policy.ExecuteAsync(() =>
            {
                executed = true;
                return Task.CompletedTask;
            });

            executed.Should()
                .BeTrue();
        }

        [Fact]
        public async Task Executing_the_policy_function_should_execute_the_specified_async_function_and_return_the_result()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { });

            var result = await policy.ExecuteAsync(() => Task.FromResult(2));

            result.Should()
                .Be(2);
        }

        #endregion

        #region ExecuteAndCapture tests

        [Fact]
        public async Task Executing_the_policy_action_successfully_should_return_success_result()
        {
            var result = await Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { })
                .ExecuteAndCaptureAsync(() => Task.CompletedTask);

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
            }, options => options.Excluding(o => o.Context));
        }

        [Fact]
        public async Task Executing_the_policy_action_and_failing_with_a_handled_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
        {
            var handledException = new DivideByZeroException();

            var result = await Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { })
                .ExecuteAndCaptureAsync(() =>
                {
                    throw handledException;
                });

            result.ShouldBeEquivalentTo(new
            {
                Outcome = OutcomeType.Failure,
                FinalException = handledException,
                ExceptionType = ExceptionType.HandledByThisPolicy,
            }, options => options.Excluding(o => o.Context));
        }

        [Fact]
        public async Task Executing_the_policy_action_and_failing_with_an_unhandled_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
        {
            var unhandledException = new Exception();

            var result = await Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { })
                .ExecuteAndCaptureAsync(() =>
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
        public async Task Executing_the_policy_function_successfully_should_return_success_result()
        {
            var result = await Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { })
                .ExecuteAndCaptureAsync(() => Task.FromResult(int.MaxValue));

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
        public async Task Executing_the_policy_function_and_failing_with_a_handled_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
        {
            var handledException = new DivideByZeroException();

            var result = await Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { })
                .ExecuteAndCaptureAsync<int>(() =>
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
        public async Task Executing_the_policy_function_and_failing_with_an_unhandled_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
        {
            var unhandledException = new Exception();

            var result = await Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { })
                .ExecuteAndCaptureAsync<int>(() =>
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

        #region Sync erroneously on async - tests

        [Theory, MemberData(nameof(SyncPolicies))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public void Executing_the_synchronous_policies_using_the_asynchronous_execute_should_throw_an_invalid_operation_exception(Policy syncPolicy, string description)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        {
            syncPolicy
                .Awaiting(async x => await x.ExecuteAsync(() => Task.CompletedTask))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.");
        }

        [Theory, MemberData(nameof(SyncPolicies))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public void Executing_the_synchronous_policies_using_the_asynchronous_execute_and_capture_should_throw_an_invalid_operation_exception(Policy syncPolicy, string description)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        {
            syncPolicy
                .Awaiting(async x => await x.ExecuteAndCaptureAsync(() => Task.CompletedTask))
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

        private static Policy RetryPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { });
        }

        private static Policy RetryForeverPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .RetryForever((_) => { });
        }

        private static Policy WaitAndRetryPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new TimeSpan[] { });
        }

        private static Policy WaitAndRetryForeverPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForever(_ => new TimeSpan());
        }

        private static Policy CircuitBreakerPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(1, new TimeSpan());
        }

        private static Policy AdvancedCircuitBreakerPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(1, TimeSpan.MaxValue, 2, new TimeSpan());
        }

        private static Policy TimeoutPolicy() => Policy
                .Timeout(TimeSpan.MaxValue);

        private static Policy BulkheadPolicy() => Policy
                .Bulkhead(1);

        private static Policy FallbackPolicy()
        {
            return Policy
                .Handle<DivideByZeroException>()
                .Fallback(() => { });
        }

        private static Policy NoOpPolicy() => Policy.NoOp();

        #endregion

        #region Context tests

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(ctx => Task.CompletedTask, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(ctx => Task.CompletedTask, null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(ctx => Task.FromResult(2), (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAsync(ctx => Task.FromResult(2), null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public async Task Executing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            var operationKey = "SomeKey";
            var executionContext = new Context(operationKey);
            Context capturedContext = null;

            Policy policy = Policy.NoOpAsync();

            await policy.ExecuteAsync((context) => { capturedContext = context; return Task.CompletedTask; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(ctx => Task.CompletedTask, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(ctx => Task.CompletedTask, null))
                .ShouldThrow<ArgumentNullException>().And
                .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(ctx => Task.FromResult(2), (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(async p => await p.ExecuteAndCaptureAsync(ctx => Task.FromResult(2), null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public async Task Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
        {
            var operationKey = "SomeKey";
            var executionContext = new Context(operationKey);
            Context capturedContext = null;

            Policy policy = Policy.NoOpAsync();

            await policy.ExecuteAndCaptureAsync((context) => { capturedContext = context; return Task.CompletedTask; }, executionContext);

            capturedContext.Should().BeSameAs(executionContext);
        }

        [Fact]
        public async Task Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
        {
            var operationKey = "SomeKey";
            var executionContext = new Context(operationKey);

            Policy policy = Policy.NoOpAsync();

            (await policy.ExecuteAndCaptureAsync((context) => Task.CompletedTask, executionContext))
                .Context.Should().BeSameAs(executionContext);
        }

        #endregion
    }
}
