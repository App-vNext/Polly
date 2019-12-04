﻿using System;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class ISyncPolicyExtensionsSpecs
    {
        [Fact]
        public void Converting_a_nongeneric_ISyncPolicy_to_generic_should_return_a_generic_ISyncPolicyTResult()
        {
            ISyncPolicy nonGenericPolicy = Policy.Timeout(10);
            var genericPolicy = nonGenericPolicy.AsPolicy<ResultClass>();

            genericPolicy.Should().BeAssignableTo<ISyncPolicy<ResultClass>>();
        }

        [Fact]
        public void Converting_a_nongeneric_ISyncPolicy_to_generic_should_return_an_ISyncPolicyTResult_version_of_the_supplied_nongeneric_policy_instance()
        {
            // Use a CircuitBreaker as a policy which we can easily manipulate to demonstrate that the executions are passing through the underlying non-generic policy.

            CircuitBreakerPolicy breaker = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero);
            ISyncPolicy nonGenericPolicy = breaker;
            var genericPolicy = nonGenericPolicy.AsPolicy<ResultPrimitive>();
            Func<ResultPrimitive> deleg = () => ResultPrimitive.Good;

            genericPolicy.Execute(deleg).Should().Be(ResultPrimitive.Good);
            breaker.Isolate();
            genericPolicy.Invoking(p => p.Execute(deleg)).Should().Throw<BrokenCircuitException>();
        }
    }

}
