using Polly.CircuitBreaker.Health;

namespace Polly.Core.Tests.CircuitBreaker.Health;

public class HealthMetricsTests
{
    [InlineData(100, typeof(SingleHealthMetrics))]
    [InlineData(199, typeof(SingleHealthMetrics))]
    [InlineData(200, typeof(RollingHealthMetrics))]
    [InlineData(201, typeof(RollingHealthMetrics))]
    [Theory]
    public void Create_Ok(int samplingDurationMs, Type expectedType) =>
        HealthMetrics.Create(
            TimeSpan.FromMilliseconds(samplingDurationMs),
            TimeProvider.System)
            .Should()
            .BeOfType(expectedType);

    [Fact]
    public void HealthInfo_WithZeroTotal_ShouldSetValuesCorrectly()
    {
        // Arrange & Act
        var result = HealthInfo.Create(0, 0);

        // Assert
        result.Throughput.Should().Be(0);
        result.FailureRate.Should().Be(0);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public void HealthInfo_ParameterizedConstructor_ShouldSetProperties()
    {
        // Arrange
        int expectedThroughput = 100;
        double expectedFailureRate = 0.25;
        int expectedFailureCount = 25;

        // Act
        var result = new HealthInfo(expectedThroughput, expectedFailureRate, expectedFailureCount);

        // Assert
        result.Throughput.Should().Be(expectedThroughput);
        result.FailureRate.Should().Be(expectedFailureRate);
        result.FailureCount.Should().Be(expectedFailureCount);
    }

    [Fact]
    public void HealthInfo_Constructor_ShouldSetValuesCorrectly()
    {
        // Arrange
        int throughput = 10;
        double failureRate = 0.2;
        int failureCount = 2;

        // Act
        var result = new HealthInfo(throughput, failureRate, failureCount);

        // Assert
        result.Throughput.Should().Be(throughput);
        result.FailureRate.Should().Be(failureRate);
        result.FailureCount.Should().Be(failureCount);
    }
}
