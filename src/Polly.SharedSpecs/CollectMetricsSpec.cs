using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;
using Polly.Shared;
using System.Linq;

namespace Polly.Specs
{
    public class CollectMetricsSpecs : IDisposable
    {
        [Fact]
        public void Should_be_able_to_be_called_on_advanced_circuit_breaker()
        {
            CircuitBreakerPolicy policy = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.MaxValue)
                .CollectMetrics("BreakerTest");
        }

        [Fact]
        public void Should_be_able_to_enumerate_collected_metric_policies()
        {
            CircuitBreakerPolicy policyMetrics1 = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.MaxValue)
                .CollectMetrics("Test 1");

            CircuitBreakerPolicy policyNoMetrics = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.MaxValue);

            CircuitBreakerPolicy policyMetrics2 = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.MaxValue)
                .CollectMetrics("Test 3");

            var actual = CollectedPolicies.All.Select(x => x.Value);
            Assert.Contains(policyMetrics1, actual);
            Assert.DoesNotContain(policyNoMetrics, actual);
            Assert.Contains(policyMetrics2, actual);
        }

        public void Dispose()
        {
        }
    }
}
