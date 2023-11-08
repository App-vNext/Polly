namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used to generate a dynamic break duration for a circuit breaker.
/// </summary>
public readonly struct BreakDurationGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BreakDurationGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="failureRate">The failure rate at which the circuit breaker should trip.
    /// It represents the ratio of failed actions to the total executed actions.</param>
    /// <param name="failureCount">The number of failures that have occurred.
    /// This count is used to determine if the failure threshold has been reached.</param>
    /// <param name="context">The resilience context providing additional information
    /// about the execution state and failures.</param>
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
