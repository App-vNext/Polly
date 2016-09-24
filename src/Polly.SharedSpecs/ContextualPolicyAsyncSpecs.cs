using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class ContextualPolicyAsyncSpecs
    {

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(p => p.ExecuteAsync(() => Task.FromResult(true), (IDictionary<string, object>) null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(p => p.ExecuteAsync(() => Task.FromResult(2), (IDictionary<string, object>) null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(p => p.ExecuteAsync(() => Task.FromResult(true), (Context) null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(p => p.ExecuteAsync(() => Task.FromResult(2), (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(p => p.ExecuteAndCaptureAsync(() => Task.FromResult(true), (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(p => p.ExecuteAndCaptureAsync(() => Task.FromResult(2), (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(p => p.ExecuteAndCaptureAsync(() => Task.FromResult(true), (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Awaiting(p => p.ExecuteAndCaptureAsync(() => Task.FromResult(2), (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }
    }
}
