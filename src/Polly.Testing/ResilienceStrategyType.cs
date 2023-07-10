namespace Polly.Testing;

/// <summary>
/// The type of resilience strategy.
/// </summary>
public enum ResilienceStrategyType
{
    /// <summary>
    /// The strategy is custome one, i.e. defined in the external library and not built-in into Polly.
    /// </summary>
    Custom,

    /// <summary>
    /// The retry strategy.
    /// </summary>
    Retry,

    /// <summary>
    /// The timeout strategy.
    /// </summary>
    Timeout,

    /// <summary>
    /// The hedging strategy.
    /// </summary>
    Hedging,

    /// <summary>
    /// The circuit breaker strategy.
    /// </summary>
    CircuitBreaker,

    /// <summary>
    /// The fallback strategy.
    /// </summary>
    Fallback,

    /// <summary>
    /// The rate limiter strategy.
    /// </summary>
    RateLimiter,

    /// <summary>
    /// The telemetry strategy.
    /// </summary>
    Telemetry,

    /// <summary>
    /// The reload strategy.
    /// </summary>
    Reload
}
