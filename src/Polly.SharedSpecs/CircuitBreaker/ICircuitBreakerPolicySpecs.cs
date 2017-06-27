using System;
using FluentAssertions;
using Polly.CircuitBreaker;
using Xunit;

namespace Polly.Specs.CircuitBreaker
{
    public class ICircuitBreakerPolicySpecs
    {
        [Fact]
        public void Should_be_able_to_use_CircuitState_via_interface()
        {
            ICircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.CircuitState.Should().Be(CircuitState.Closed);

        }

        [Fact]
        public void Should_be_able_to_use_Isolate_via_interface()
        {
            ICircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
        }

        [Fact]
        public void Should_be_able_to_use_Reset_via_interface()
        {
            ICircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            breaker.Reset();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_be_able_to_use_LastException_via_interface()
        {
            ICircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.LastException.Should().BeNull();

        }

    }
}
