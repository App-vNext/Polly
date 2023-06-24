using Polly.CircuitBreaker.Health;

namespace Polly.Core.Tests.CircuitBreaker.Health;
public class SingleHealthMetricsTests
{
    private readonly MockTimeProvider _timeProvider;

    public SingleHealthMetricsTests() => _timeProvider = new MockTimeProvider().SetupUtcNow();

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var metrics = new SingleHealthMetrics(TimeSpan.FromMilliseconds(100), _timeProvider.Object);
        var health = metrics.GetHealthInfo();

        health.FailureRate.Should().Be(0);
        health.Throughput.Should().Be(0);
    }

    [Fact]
    public void Increment_Ok()
    {
        var metrics = new SingleHealthMetrics(TimeSpan.FromMilliseconds(100), _timeProvider.Object);

        metrics.IncrementFailure();
        metrics.IncrementSuccess();
        metrics.IncrementSuccess();
        metrics.IncrementSuccess();
        metrics.IncrementSuccess();

        var health = metrics.GetHealthInfo();

        health.FailureRate.Should().Be(0.2);
        health.Throughput.Should().Be(5);
    }

    [Fact]
    public void Reset_Ok()
    {
        var metrics = new SingleHealthMetrics(TimeSpan.FromMilliseconds(100), _timeProvider.Object);

        metrics.IncrementSuccess();
        metrics.Reset();

        metrics.GetHealthInfo().Throughput.Should().Be(0);
    }

    [Fact]
    public void SamplingDurationRespected_Ok()
    {
        var metrics = new SingleHealthMetrics(TimeSpan.FromMilliseconds(100), _timeProvider.Object);

        metrics.IncrementSuccess();
        metrics.IncrementSuccess();

        _timeProvider.AdvanceTime(TimeSpan.FromMilliseconds(100));

        metrics.GetHealthInfo().Throughput.Should().Be(0);
    }
}
