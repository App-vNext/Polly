using Polly.CircuitBreaker.Health;

namespace Polly.CircuitBreaker;

/// <summary>
/// Defines the behavior of circuit breaker. All methods on this class are performed under a lock.
/// </summary>
internal abstract class CircuitBehavior
{
    public abstract void OnActionSuccess(CircuitState currentState);

    public abstract void OnActionFailure(CircuitState currentState, out bool shouldBreak);

    public abstract void OnCircuitClosed();

    public abstract HealthInfo GetHealthInfo();
}
