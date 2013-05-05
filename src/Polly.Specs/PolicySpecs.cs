using System;
using FluentAssertions;
using Xunit;

namespace Polly.Specs
{
    public class PolicySpecs
    {
        [Fact]
        public void Can_call_execute_on_a_policy_to_get_the_result_of_executing_an_action()
        {
            var policy = Policy
                          .Handle<DivideByZeroException>()
                          .Retry();

            var result = policy.Execute(() => 2);

            result.Should()
                  .Be(2);
        } 
    }
}