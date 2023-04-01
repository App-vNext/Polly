using Polly.Strategy;

namespace Polly.Retry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents the arguments used in <see cref="ShouldRetryPredicate"/> for determining whether a retry should be performed.
/// </summary>
public readonly struct ShouldRetryArguments : IResilienceArguments
{
    internal ShouldRetryArguments(ResilienceContext context, int attemptNumber, TimeSpan totalExecutionTime)
    {
        AttemptNumber = attemptNumber;
        TotalExecutionTime = totalExecutionTime;
        Context = context;
    }

    /// <summary>
    /// Gets the zero-based attempt number.
    /// </summary>
    /// <remarks>
    /// The first attempt is 0, the second attempt is 1, and so on.
    /// </remarks>
    public int AttemptNumber { get; }

    /// <summary>
    /// Gets the total execution time.
    /// </summary>
    /// <remarks>
    /// The total execution time from the very first attempt and up to this point.
    /// </remarks>
    public TimeSpan TotalExecutionTime { get; }

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
