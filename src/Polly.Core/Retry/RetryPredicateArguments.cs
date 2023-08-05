namespace Polly.Retry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.ShouldHandle"/> for determining whether a retry should be performed.
/// </summary>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct RetryPredicateArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryPredicateArguments"/> struct.
    /// </summary>
    /// <param name="attemptNumber">The zero-based attempt number.</param>
    public RetryPredicateArguments(int attemptNumber) => AttemptNumber = attemptNumber;

    /// <summary>
    /// Gets the zero-based attempt number.
    /// </summary>
    public int AttemptNumber { get; }
}
