﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
{
    public class IAsyncPolicyExtensionsSpecs
    {
        [Fact]
        public void Converting_a_nongeneric_IAsyncPolicy_to_generic_should_return_a_generic_IAsyncPolicyTResult()
        {
            IAsyncPolicy nonGenericPolicy = Policy.TimeoutAsync(10);
            var genericPolicy = nonGenericPolicy.AsAsyncPolicy<ResultClass>();

            genericPolicy.Should().BeAssignableTo<IAsyncPolicy<ResultClass>>();
        }

        [Fact]
        public async Task Converting_a_nongeneric_IAsyncPolicy_to_generic_should_return_an_IAsyncPolicyTResult_version_of_the_supplied_nongeneric_policy_instance()
        {
            // Use a CircuitBreaker as a policy which we can easily manipulate to demonstrate that the executions are passing through the underlying non-generic policy.

            var breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.Zero);
            IAsyncPolicy nonGenericPolicy = breaker;
            var genericPolicy = nonGenericPolicy.AsAsyncPolicy<ResultPrimitive>();
            Func<Task<ResultPrimitive>> deleg = () => Task.FromResult(ResultPrimitive.Good);

            (await genericPolicy.ExecuteAsync(deleg)).Should().Be(ResultPrimitive.Good);
            breaker.Isolate();
            genericPolicy.Awaiting(p => p.ExecuteAsync(deleg)).Should().Throw<BrokenCircuitException>();
        }
    }

}
