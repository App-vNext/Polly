using System;
using Polly.CircuitBreaker.Health;
using Polly.Utils;

namespace Polly.Core.Tests.CircuitBreaker.Health;

public class HealthMetricsTests
{
    [InlineData(100, typeof(SingleHealthMetrics))]
    [InlineData(199, typeof(SingleHealthMetrics))]
    [InlineData(200, typeof(RollingHealthMetrics))]
    [InlineData(201, typeof(RollingHealthMetrics))]
    [Theory]
    public void Create_Ok(int samplingDurationMs, Type expectedType)
    {
        HealthMetrics.Create(
            TimeSpan.FromMilliseconds(samplingDurationMs),
            TimeProvider.System)
            .Should()
            .BeOfType(expectedType);
    }
}
