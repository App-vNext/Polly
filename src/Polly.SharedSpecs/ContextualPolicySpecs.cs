using System;
using System.Collections.Generic;
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

            policy.Invoking(p => p.Execute(() => { }, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
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
        public void Executing_the_policy_function_should_throw_when_context_data_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => 2, (IDictionary<string, object>)null))
                  .ShouldThrow<ArgumentNullException>();
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
        public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
        {
            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.ExecuteAndCapture(() => 2, (Context)null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("context");
        }
    }
}