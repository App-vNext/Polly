using System;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class PolicySpecs
    {
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
    }
}