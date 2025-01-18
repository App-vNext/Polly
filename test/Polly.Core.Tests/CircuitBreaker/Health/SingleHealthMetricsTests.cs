using Microsoft.Extensions.Time.Testing;
using Polly.CircuitBreaker.Health;

namespace Polly.Core.Tests.CircuitBreaker.Health;

public class SingleHealthMetricsTests
{
    private readonly FakeTimeProvider _timeProvider;

    public SingleHealthMetricsTests() => _timeProvider = new FakeTimeProvider();

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var metrics = new SingleHealthMetrics(TimeSpan.FromMilliseconds(100), _timeProvider);
        var health = metrics.GetHealthInfo();

        health.FailureRate.ShouldBe(0);
        health.Throughput.ShouldBe(0);
    }

    [Fact]
    public void Increment_Ok()
    {
        var metrics = new SingleHealthMetrics(TimeSpan.FromMilliseconds(100), _timeProvider);

        metrics.IncrementFailure();
        metrics.IncrementSuccess();
        metrics.IncrementSuccess();
        metrics.IncrementSuccess();
        metrics.IncrementSuccess();

        var health = metrics.GetHealthInfo();

        health.FailureRate.ShouldBe(0.2);
        health.Throughput.ShouldBe(5);
    }

    [Fact]
    public void Reset_Ok()
    {
        var metrics = new SingleHealthMetrics(TimeSpan.FromMilliseconds(100), _timeProvider);

        metrics.IncrementSuccess();
        metrics.Reset();

        metrics.GetHealthInfo().Throughput.ShouldBe(0);
    }

    [Fact]
    public void SamplingDurationRespected_Ok()
    {
        var metrics = new SingleHealthMetrics(TimeSpan.FromMilliseconds(100), _timeProvider);

        metrics.IncrementSuccess();
        metrics.IncrementSuccess();

        _timeProvider.Advance(TimeSpan.FromMilliseconds(100));

        metrics.GetHealthInfo().Throughput.ShouldBe(0);
    }
}
