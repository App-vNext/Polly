namespace Polly.Specs.CircuitBreaker;

[Collection(Constants.SystemClockDependentTestCollection)]
public class RollingHealthMetricsTests : IDisposable
{
    private readonly TimeSpan _samplingDuration = TimeSpan.FromSeconds(10);
    private DateTime _utcNow = new(2025, 02, 16, 12, 34, 56, DateTimeKind.Utc);

    public RollingHealthMetricsTests() => SystemClock.UtcNow = () => _utcNow;

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var metrics = Create();
        var health = metrics.GetHealthCount_NeedsLock();
        health.Successes.ShouldBe(0);
        health.Failures.ShouldBe(0);
    }

    [Fact]
    public void Increment_Ok()
    {
        var metrics = Create();

        metrics.IncrementFailure_NeedsLock();
        metrics.IncrementSuccess_NeedsLock();
        metrics.IncrementSuccess_NeedsLock();
        metrics.IncrementSuccess_NeedsLock();
        metrics.IncrementSuccess_NeedsLock();

        var health = metrics.GetHealthCount_NeedsLock();

        health.Failures.ShouldBe(1);
        health.Successes.ShouldBe(4);
    }

    [Fact]
    public void GetHealthCount_NeedsLock_EnsureWindowRespected()
    {
        var metrics = Create();
        var health = new List<HealthCount>();

        var startedAt = _utcNow;

        for (int i = 0; i < 5; i++)
        {
            if (i < 2)
            {
                metrics.IncrementFailure_NeedsLock();
            }
            else
            {
                metrics.IncrementSuccess_NeedsLock();
            }

            metrics.IncrementSuccess_NeedsLock();
            _utcNow += TimeSpan.FromSeconds(2);
            health.Add(metrics.GetHealthCount_NeedsLock());
        }

        _utcNow += TimeSpan.FromSeconds(2);
        health.Add(metrics.GetHealthCount_NeedsLock());

        health[0].ShouldBeEquivalentTo(new HealthCount { Successes = 1, Failures = 1, StartedAt = startedAt.AddSeconds(0).Ticks });
        health[1].ShouldBeEquivalentTo(new HealthCount { Successes = 2, Failures = 2, StartedAt = startedAt.AddSeconds(0).Ticks });
        health[2].ShouldBeEquivalentTo(new HealthCount { Successes = 4, Failures = 2, StartedAt = startedAt.AddSeconds(0).Ticks });
        health[3].ShouldBeEquivalentTo(new HealthCount { Successes = 6, Failures = 2, StartedAt = startedAt.AddSeconds(0).Ticks });
        health[4].ShouldBeEquivalentTo(new HealthCount { Successes = 7, Failures = 1, StartedAt = startedAt.AddSeconds(2).Ticks });
        health[5].ShouldBeEquivalentTo(new HealthCount { Successes = 6, Failures = 0, StartedAt = startedAt.AddSeconds(4).Ticks });
    }

    [Fact]
    public void GetHealthCount_NeedsLock_EnsureWindowCapacityRespected()
    {
        var delay = TimeSpan.FromSeconds(1);
        var metrics = Create();

        for (int i = 0; i < 10; i++)
        {
            metrics.IncrementSuccess_NeedsLock();
            _utcNow += delay;
        }

        metrics.GetHealthCount_NeedsLock().Successes.ShouldBe(9);
        _utcNow += delay;
        metrics.GetHealthCount_NeedsLock().Successes.ShouldBe(8);
    }

    [Fact]
    public void Reset_Ok()
    {
        var metrics = Create();

        metrics.IncrementSuccess_NeedsLock();
        metrics.Reset_NeedsLock();

        _utcNow += _samplingDuration;
        _utcNow += _samplingDuration;

        metrics.GetHealthCount_NeedsLock().Successes.ShouldBe(0);

        _utcNow += _samplingDuration;
        _utcNow += _samplingDuration;

        metrics.GetHealthCount_NeedsLock().Successes.ShouldBe(0);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void GetHealthCount_NeedsLock_SamplingDurationRespected(bool variance)
    {
        var metrics = Create();

        metrics.IncrementSuccess_NeedsLock();
        metrics.IncrementSuccess_NeedsLock();

        _utcNow += _samplingDuration + (variance ? TimeSpan.FromMilliseconds(1) : TimeSpan.Zero);

        metrics.GetHealthCount_NeedsLock().ShouldBeEquivalentTo(new HealthCount { StartedAt = _utcNow.Ticks });
    }

    private RollingHealthMetrics Create() => new(_samplingDuration);

    public void Dispose() => SystemClock.Reset();
}
