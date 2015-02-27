using System;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class PolicySpecs
    {
        [Fact]
        public void Executing_the_synchronous_policy_using_the_asynchronous_retry_should_throw_an_invalid_operation_exception()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { })
                .Execute(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");
        }

        [Fact]
        public void Executing_the_synchronous_policy_using_the_asynchronous_retry_forever_should_throw_an_invalid_operation_exception()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync((_) => { })
                .Execute(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");
        }

        [Fact]
        public void Executing_the_synchronous_policy_using_the_asynchronous_wait_and_retry_should_throw_an_invalid_operation_exception()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryAsync(new TimeSpan[] {})
                .Execute(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");
        }

        [Fact]
        public void Executing_the_synchronous_policy_using_the_asynchronous_circuit_breaker_should_throw_an_invalid_operation_exception()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(1, new TimeSpan())
                .Execute(() => { });

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the synchronous Retry, RetryForever, WaitAndRetry or CircuitBreaker methods when calling the synchronous Execute method.");
        }

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
        public void Executing_the_policy_function_for_typeof_should_execute_the_specified_function_and_return_the_result()
        {
            var executed = false;

            var policy = Policy
                          .Handle(typeof(DivideByZeroException))
                          .Retry((_, __) => { });

            policy.Execute(() => executed = true);

            executed.Should()
                    .BeTrue();
        }

        [Fact]
        public void Executing_the_policy_function_for_exception_type_should_execute_the_specified_function_and_return_the_result()
        {
            var policy = Policy
                          .Handle(typeof(DivideByZeroException))
                          .Retry((_, __) => { });

            var result = policy.Execute(() => 2);

            result.Should()
                  .Be(2);
        }
    }
}