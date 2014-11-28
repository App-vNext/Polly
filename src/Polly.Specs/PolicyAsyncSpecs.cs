using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class PolicyAsyncSpecs
    {
        public static readonly Task CompletedTask = Task.FromResult(0);

        [Fact]
        public void Executing_the_async_policy_using_the_synchronous_retry_should_throw_an_invalid_operation_exception()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __) => { });

            policy.Awaiting(x => x.ExecuteAsync(() => CompletedTask))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");
        }

        [Fact]
        public void Executing_the_async_policy_using_the_synchronous_retry_forever_should_throw_an_invalid_operation_exception()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForever(_ => { });

            policy.Awaiting(x => x.ExecuteAsync(() => CompletedTask))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");
        }

        [Fact]
        public void Executing_the_async_policy_using_the_synchronous_wait_and_retry_should_throw_an_invalid_operation_exception()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(Enumerable.Empty<TimeSpan>());

            policy.Awaiting(x => x.ExecuteAsync(() => CompletedTask))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");
        }

        [Fact]
        public void Executing_the_async_policy_using_the_synchronous_circuit_breaker_should_throw_an_invalid_operation_exception()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(1, new TimeSpan());

            policy.Awaiting(x => x.ExecuteAsync(() => CompletedTask))
                .ShouldThrow<InvalidOperationException>()
                .WithMessage("Please use the asynchronous RetryAsync, RetryForeverAsync, WaitAndRetryAsync or CircuitBreakerAsync methods when calling the asynchronous Execute method.");
        }

        [Fact]
        public async Task Executing_the_policy_action_should_execute_the_specified_async_action()
        {
            bool executed = false;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __) => { });

            await policy.ExecuteAsync(() =>
            {
                executed = true;
                return Task.FromResult(true) as Task;
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

            int result = await policy.ExecuteAsync(() => Task.FromResult(2));

            result.Should()
                .Be(2);
        }
    }
}