using Microsoft.Extensions.Time.Testing;
using Polly.CircuitBreaker.Health;

namespace Polly.Core.Tests.CircuitBreaker.Health;

public class RollingHealthMetricsTests
{
    private readonly FakeTimeProvider _timeProvider;
    private readonly TimeSpan _samplingDuration = TimeSpan.FromSeconds(10);
    private readonly short _windows = 10;

    public RollingHealthMetricsTests() => _timeProvider = new FakeTimeProvider();

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var metrics = Create();
        var health = metrics.GetHealthInfo();
        health.FailureRate.Should().Be(0);
        health.Throughput.Should().Be(0);
    }

    [Fact]
    public void Increment_Ok()
    {
        var metrics = Create();

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
    public void GetHealthInfo_EnsureWindowRespected()
    {
        var metrics = Create();
        var health = new List<HealthInfo>();

        for (int i = 0; i < 5; i++)
        {
            if (i < 2)
            {
                metrics.IncrementFailure();
            }
            else
            {
                metrics.IncrementSuccess();
            }

            metrics.IncrementSuccess();
            _timeProvider.Advance(TimeSpan.FromSeconds(2));
            health.Add(metrics.GetHealthInfo());
        }

        _timeProvider.Advance(TimeSpan.FromSeconds(2));
        health.Add(metrics.GetHealthInfo());

        health[0].Should().Be(new HealthInfo(2, 0.5, 1));
        health[1].Should().Be(new HealthInfo(4, 0.5, 2));
        health[3].Should().Be(new HealthInfo(8, 0.25, 2));
        health[4].Should().Be(new HealthInfo(8, 0.125,1));
        health[5].Should().Be(new HealthInfo(6, 0.0,0));
    }

    [Fact]
    public void GetHealthInfo_EnsureWindowCapacityRespected()
    {
        var delay = TimeSpan.FromSeconds(1);
        var metrics = Create();

        for (int i = 0; i < _windows; i++)
        {
            metrics.IncrementSuccess();
            _timeProvider.Advance(delay);
        }

        metrics.GetHealthInfo().Throughput.Should().Be(9);
        _timeProvider.Advance(delay);
        metrics.GetHealthInfo().Throughput.Should().Be(8);
    }

    [Fact]
    public void Reset_Ok()
    {
        var metrics = Create();

        metrics.IncrementSuccess();
        metrics.Reset();

        metrics.GetHealthInfo().Throughput.Should().Be(0);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void GetHealthInfo_SamplingDurationRespected(bool variance)
    {
        var metrics = Create();

        metrics.IncrementSuccess();
        metrics.IncrementSuccess();

        _timeProvider.Advance(_samplingDuration + (variance ? TimeSpan.FromMilliseconds(1) : TimeSpan.Zero));

        metrics.GetHealthInfo().Should().Be(new HealthInfo(0, 0));
    }

    private RollingHealthMetrics Create() => new(_samplingDuration, _windows, _timeProvider);
}
