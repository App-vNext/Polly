#nullable enable
namespace Polly.CircuitBreaker;

internal sealed class SingleHealthMetrics : IHealthMetrics
{
    private readonly long _samplingDuration;

    private HealthCount? _current;

    public SingleHealthMetrics(TimeSpan samplingDuration) =>
        _samplingDuration = samplingDuration.Ticks;

    public void IncrementSuccess_NeedsLock() =>
        ActualiseCurrentMetric_NeedsLock().Successes++;

    public void IncrementFailure_NeedsLock() =>
        ActualiseCurrentMetric_NeedsLock().Failures++;

    public void Reset_NeedsLock() =>
        _current = null;

    public HealthCount GetHealthCount_NeedsLock() =>
        ActualiseCurrentMetric_NeedsLock();

    private HealthCount ActualiseCurrentMetric_NeedsLock()
    {
        long now = SystemClock.UtcNow().Ticks;
        if (_current == null || now - _current.StartedAt >= _samplingDuration)
        {
            _current = new()
            {
                StartedAt = now
            };
        }

        return _current;
    }
}
