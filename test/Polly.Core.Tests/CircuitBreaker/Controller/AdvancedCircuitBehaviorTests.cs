using NSubstitute;
using Polly.CircuitBreaker;
using Polly.CircuitBreaker.Health;

namespace Polly.Core.Tests.CircuitBreaker.Controller;

public class AdvancedCircuitBehaviorTests
{
    private HealthMetrics _metrics = Substitute.For<HealthMetrics>(TimeProvider.System);

    [InlineData(10, 10, 0.0, 0.1, false)]
    [InlineData(10, 10, 0.1, 0.1, true)]
    [InlineData(10, 10, 0.2, 0.1, true)]
    [InlineData(11, 10, 0.2, 0.1, true)]
    [InlineData(9, 10, 0.1, 0.1, false)]
    [Theory]
    public void OnActionFailure_WhenClosed_EnsureCorrectBehavior(
        int throughput,
        int minimumThruput,
        double failureRate,
        double failureThreshold,
        bool expectedShouldBreak)
    {
        _metrics.GetHealthInfo().Returns(new HealthInfo(throughput, failureRate));

        var behavior = new AdvancedCircuitBehavior(failureThreshold, minimumThruput, _metrics);

        behavior.OnActionFailure(CircuitState.Closed, out var shouldBreak);

        shouldBreak.Should().Be(expectedShouldBreak);
        _metrics.Received(1).IncrementFailure();
    }

    [InlineData(CircuitState.Closed, true)]
    [InlineData(CircuitState.Open, true)]
    [InlineData(CircuitState.Isolated, true)]
    [InlineData(CircuitState.HalfOpen, false)]
    [Theory]
    public void OnActionFailure_State_EnsureCorrectCalls(CircuitState state, bool shouldIncrementFailure)
    {
        _metrics = Substitute.For<HealthMetrics>(TimeProvider.System);

        var sut = Create();

        sut.OnActionFailure(state, out var shouldBreak);

        shouldBreak.Should().BeFalse();
        if (shouldIncrementFailure)
        {
            _metrics.Received(1).IncrementFailure();
        }
        else
        {
            _metrics.DidNotReceive().IncrementFailure();
        }
    }

    [Fact]
    public void OnCircuitClosed_Ok()
    {
        _metrics = Substitute.For<HealthMetrics>(TimeProvider.System);
        var sut = Create();

        sut.OnCircuitClosed();

        _metrics.Received(1).Reset();
    }

    private AdvancedCircuitBehavior Create()
    {
        return new(CircuitBreakerConstants.DefaultFailureRatio, CircuitBreakerConstants.DefaultMinimumThroughput, _metrics);
    }
}
