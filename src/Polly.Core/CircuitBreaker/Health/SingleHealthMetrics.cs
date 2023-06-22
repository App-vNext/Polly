namespace Polly.CircuitBreaker.Health;

/// <inheritdoc/>
internal sealed class SingleHealthMetrics : HealthMetrics
{
    private readonly TimeSpan _samplingDuration;

    private int _successes;
    private int _failures;
    private DateTimeOffset _startedAt;

    public SingleHealthMetrics(TimeSpan samplingDuration, TimeProvider timeProvider)
        : base(timeProvider)
    {
        _samplingDuration = samplingDuration;
        _startedAt = timeProvider.GetUtcNow();
    }

    public override void IncrementSuccess()
    {
        TryReset();
        _successes++;
    }

    public override void IncrementFailure()
    {
        TryReset();
        _failures++;
    }

    public override void Reset()
    {
        _startedAt = TimeProvider.GetUtcNow();
        _successes = 0;
        _failures = 0;
    }

    public override HealthInfo GetHealthInfo()
    {
        TryReset();

        return HealthInfo.Create(_successes, _failures);
    }

    private void TryReset()
    {
        if (TimeProvider.GetUtcNow() - _startedAt >= _samplingDuration)
        {
            Reset();
        }
    }
}
