namespace Polly.CircuitBreaker;

/// <summary>
/// Represents arguments used to generate a dynamic break duration for a circuit breaker.
/// </summary>
public class BreakDurationGeneratorArguments
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
    /// The failure rate that represents the ratio of failures to total actions.
    /// </summary>
    public double FailureRate { get; set; }

    /// <summary>
    /// The count of failures that have occurred.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// The context that provides additional information about the resilience operation.
    /// </summary>
    public ResilienceContext Context { get; set; }
}
