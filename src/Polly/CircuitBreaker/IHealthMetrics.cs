namespace Polly.CircuitBreaker;

/// <summary>
/// Interface for managing health metrics.
/// </summary>
internal interface IHealthMetrics
{
    /// <summary>
    /// Increments the success count.
    /// </summary>
    void IncrementSuccess_NeedsLock();

    /// <summary>
    /// Increments the failure count.
    /// </summary>
    void IncrementFailure_NeedsLock();

    /// <summary>
    /// Resets the health metrics.
    /// </summary>
    void Reset_NeedsLock();

    /// <summary>
    /// Gets the health count.
    /// </summary>
    /// <returns>The current health count.</returns>
    HealthCount GetHealthCount_NeedsLock();
}
