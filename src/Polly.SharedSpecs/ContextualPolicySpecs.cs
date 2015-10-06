using System;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class ContextualPolicySpecs
    {
        [Fact]
        public void Executing_the_policy_action_should_execute_the_specified_action()
        {
            var executed = false;

            var policy = Policy
                          .Handle<DivideByZeroException>()
                          .Retry((_, __, ___) => { });

            policy.Execute(() => executed = true);

            executed.Should()
                    .BeTrue();
        } 

        [Fact]
        public void Executing_the_policy_function_should_execute_the_specified_function_and_return_the_result()
        {
            var policy = Policy
                          .Handle<DivideByZeroException>()
                          .Retry((_, __, ___) => { });

            var result = policy.Execute(() => 2);

            result.Should()
                  .Be(2);
        } 

        [Fact]
        public void Executing_the_policy_action_should_should_throw_when_context_data_is_null()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => { }, null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }

        [Fact]
        public void Executing_the_policy_function_should_should_throw_when_context_data_is_null()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, ___) => { });

            policy.Invoking(p => p.Execute(() => 2, null))
                  .ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("contextData");
        }
    }
}