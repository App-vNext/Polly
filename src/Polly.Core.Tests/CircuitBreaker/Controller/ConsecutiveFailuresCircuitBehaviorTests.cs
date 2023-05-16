using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker.Controller;
public class ConsecutiveFailuresCircuitBehaviorTests
{
    [Fact]
    public void OnCircuitReset_Ok()
    {
        var behavior = new ConsecutiveFailuresCircuitBehavior(new SimpleCircuitBreakerStrategyOptions { FailureThreshold = 2 });

        behavior.OnActionFailure(CircuitState.Closed, out var shouldBreak);
        behavior.OnCircuitClosed();
        behavior.OnActionFailure(CircuitState.Closed, out shouldBreak);

        shouldBreak.Should().BeFalse();
    }

    [InlineData(1, 1, true)]
    [InlineData(2, 1, false)]
    [Theory]
    public void OnActionFailure_Ok(int threshold, int failures, bool expectedShouldBreak)
    {
        var behavior = new ConsecutiveFailuresCircuitBehavior(new SimpleCircuitBreakerStrategyOptions { FailureThreshold = threshold });

        for (int i = 0; i < failures - 1; i++)
        {
            behavior.OnActionFailure(CircuitState.Closed, out _);
        }

        behavior.OnActionFailure(CircuitState.Closed, out var shouldBreak);
        shouldBreak.Should().Be(expectedShouldBreak);
    }

    [InlineData(CircuitState.Closed, false)]
    [InlineData(CircuitState.Open, true)]
    [InlineData(CircuitState.Isolated, true)]
    [InlineData(CircuitState.HalfOpen, true)]
    [Theory]
    public void OnActionSuccess_Ok(CircuitState state, bool expected)
    {
        var behavior = new ConsecutiveFailuresCircuitBehavior(new SimpleCircuitBreakerStrategyOptions { FailureThreshold = 2 });

        behavior.OnActionFailure(CircuitState.Closed, out var shouldBreak);
        behavior.OnActionSuccess(state);
        behavior.OnActionFailure(CircuitState.Closed, out shouldBreak);

        shouldBreak.Should().Be(expected);
    }
}
