namespace Polly.CircuitBreaker.Health;

/// <summary>
/// The health metrics for advanced circuit breaker.
/// All operations here are executed from <see cref="CircuitStateController{T}"/> under a lock and are thread safe.
/// </summary>
internal abstract class HealthMetrics
{
    private const short NumberOfWindows = 10;
    private static readonly TimeSpan ResolutionOfCircuitTimer = TimeSpan.FromMilliseconds(20);

    protected HealthMetrics(TimeProvider timeProvider) => TimeProvider = timeProvider;

    public static HealthMetrics Create(TimeSpan samplingDuration, TimeProvider timeProvider)
        => samplingDuration < TimeSpan.FromTicks(ResolutionOfCircuitTimer.Ticks * NumberOfWindows)
           ? new SingleHealthMetrics(samplingDuration, timeProvider)
           : new RollingHealthMetrics(samplingDuration, NumberOfWindows, timeProvider);

    protected TimeProvider TimeProvider { get; }

    public abstract void IncrementSuccess();

    public abstract void IncrementFailure();

    public abstract void Reset();

    public abstract HealthInfo GetHealthInfo();
}
