using NSubstitute;
using Polly.CircuitBreaker;
using Polly.CircuitBreaker.Health;

namespace Polly.Core.Tests.CircuitBreaker.Controller;

public class AdvancedCircuitBehaviorTests
{
    private HealthMetrics _metrics = Substitute.For<HealthMetrics>(TimeProvider.System);

    [InlineData(10, 10, 0.0, 0.1, 0, false)]
    [InlineData(10, 10, 0.1, 0.1, 1, true)]
    [InlineData(10, 10, 0.2, 0.1, 2, true)]
    [InlineData(11, 10, 0.2, 0.1, 3, true)]
    [InlineData(9, 10, 0.1, 0.1, 4, false)]
    [Theory]
    public void OnActionFailure_WhenClosed_EnsureCorrectBehavior(
        int throughput,
        int minimumThruput,
        double failureRate,
        double failureThreshold,
        int failureCount,
        bool expectedShouldBreak)
    {
        _metrics.GetHealthInfo().Returns(new HealthInfo(throughput, failureRate, failureCount));

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

    [Theory]
    [InlineData(10, 0.0, 0)]
    [InlineData(10, 0.1, 1)]
    [InlineData(10, 0.2, 2)]
    [InlineData(11, 0.2, 3)]
    [InlineData(9, 0.1, 4)]
    public void BehaviorProperties_ShouldReflectHealthInfoValues(
        int throughput,
        double failureRate,
        int failureCount)
    {
        var anyFailureThreshold = 10;
        var anyMinimumThruput = 100;

        _metrics.GetHealthInfo().Returns(new HealthInfo(throughput, failureRate, failureCount));
        var behavior = new AdvancedCircuitBehavior(anyFailureThreshold, anyMinimumThruput, _metrics);

        behavior.FailureCount.Should().Be(failureCount, "because the FailureCount should match the HealthInfo");
        behavior.FailureRate.Should().Be(failureRate, "because the FailureRate should match the HealthInfo");
    }

    private AdvancedCircuitBehavior Create() =>
        new(CircuitBreakerConstants.DefaultFailureRatio, CircuitBreakerConstants.DefaultMinimumThroughput, _metrics);
}
