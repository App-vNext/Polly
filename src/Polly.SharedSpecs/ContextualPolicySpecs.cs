using System;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class ContextualPolicySpecs
    {

        [Fact]
        public void Executing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => { }, null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }
        
        [Fact]
        public void Execute_and_capturing_the_policy_action_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(() => { }, null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }

        [Fact]
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => 2, null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }

        [Fact]
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(() => 2, null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }
    }
}