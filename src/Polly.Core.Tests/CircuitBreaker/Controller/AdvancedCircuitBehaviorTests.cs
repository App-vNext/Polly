using Moq;
using Polly.CircuitBreaker;
using Polly.CircuitBreaker.Health;
using Polly.Utils;

namespace Polly.Core.Tests.CircuitBreaker.Controller;

public class AdvancedCircuitBehaviorTests
{
    private Mock<HealthMetrics> _metrics = new(MockBehavior.Strict, TimeProvider.System);

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
        _metrics.Setup(m => m.IncrementFailure());
        _metrics.Setup(m => m.GetHealthInfo()).Returns(new HealthInfo(throughput, failureRate));

        var behavior = new AdvancedCircuitBehavior(failureThreshold, minimumThruput, _metrics.Object);

        behavior.OnActionFailure(CircuitState.Closed, out var shouldBreak);

        shouldBreak.Should().Be(expectedShouldBreak);
        _metrics.VerifyAll();
    }

    [InlineData(CircuitState.Closed, true)]
    [InlineData(CircuitState.Open, true)]
    [InlineData(CircuitState.Isolated, true)]
    [InlineData(CircuitState.HalfOpen, false)]
    [Theory]
    public void OnActionFailure_State_EnsureCorrectCalls(CircuitState state, bool shouldIncrementFailure)
    {
        _metrics = new(MockBehavior.Loose, TimeProvider.System);

        var sut = Create();

        sut.OnActionFailure(state, out var shouldBreak);

        shouldBreak.Should().BeFalse();
        if (shouldIncrementFailure)
        {
            _metrics.Verify(v => v.IncrementFailure(), Times.Once());
        }
        else
        {
            _metrics.Verify(v => v.IncrementFailure(), Times.Never());
        }
    }

    [Fact]
    public void OnCircuitClosed_Ok()
    {
        _metrics = new(MockBehavior.Loose, TimeProvider.System);
        var sut = Create();

        sut.OnCircuitClosed();

        _metrics.Verify(v => v.Reset(), Times.Once());
    }

    private AdvancedCircuitBehavior Create()
    {
        return new AdvancedCircuitBehavior(CircuitBreakerConstants.DefaultAdvancedFailureThreshold, CircuitBreakerConstants.DefaultMinimumThroughput, _metrics.Object);
    }
}
