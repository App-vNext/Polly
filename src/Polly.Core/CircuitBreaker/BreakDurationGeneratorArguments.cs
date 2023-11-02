namespace Polly.CircuitBreaker;

/// <summary>
/// Represents arguments used to generate a dynamic break duration for a circuit breaker.
/// </summary>
public readonly struct BreakDurationGeneratorArguments
{
    public BreakDurationGeneratorArguments(
        double failureRate,
        int failureCount,
        ResilienceContext context)
    {
        FailureRate = failureRate;
        FailureCount = failureCount;
        Context = context;
    }

    /// <summary>
    /// Gets the failure rate that represents the ratio of failures to total actions.
    /// </summary>
    public double FailureRate { get; }

    /// <summary>
    /// Gets the count of failures that have occurred.
    /// </summary>
    public int FailureCount { get; }

    /// <summary>
    /// Gets the context that provides additional information about the resilience operation.
    /// </summary>
    public ResilienceContext Context { get; }
}
