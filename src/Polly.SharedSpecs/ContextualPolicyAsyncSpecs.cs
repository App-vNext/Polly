using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
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

            policy.Invoking(p => p.ExecuteAsync(() => Task.FromResult(true), null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAsync(() => Task.FromResult(2), null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCaptureAsync(() => Task.FromResult(true), null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryAsync((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCaptureAsync(() => Task.FromResult(2), null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }
    }
}
