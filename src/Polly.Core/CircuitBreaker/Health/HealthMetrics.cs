namespace Polly.CircuitBreaker.Health;

internal abstract class HealthMetrics
{
    private const short NumberOfWindows = 10;
    private static readonly TimeSpan ResolutionOfCircuitTimer = TimeSpan.FromMilliseconds(20);

    protected HealthMetrics(TimeProvider timeProvider) => TimeProvider = timeProvider;

    public static HealthMetrics Create(AdvancedCircuitBreakerStrategyOptions options, TimeProvider timeProvider)
    {
        return options.SamplingDuration < TimeSpan.FromTicks(ResolutionOfCircuitTimer.Ticks * NumberOfWindows)
            ? new SingleHealthMetrics(options.SamplingDuration, timeProvider)
            : new RollingHealthMetrics(options.SamplingDuration, NumberOfWindows, timeProvider);
    }

    protected TimeProvider TimeProvider { get; }

    public abstract void IncrementSuccess();

    public abstract void IncrementFailure();

    public abstract void Reset();

    public abstract HealthInfo GetHealthInfo();
}
