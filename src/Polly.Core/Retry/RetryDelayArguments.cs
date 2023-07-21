namespace Polly.Retry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.RetryDelayGenerator"/> for generating the next retry delay.
/// </summary>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct RetryDelayArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryDelayArguments"/> struct.
    /// </summary>
    /// <param name="attempt">The zero-based attempt number.</param>
    /// <param name="delayHint">The delay suggested by the retry strategy.</param>
    public RetryDelayArguments(int attempt, TimeSpan delayHint)
    {
        Attempt = attempt;
        DelayHint = delayHint;
    }

    /// <summary>
    /// Gets The zero-based attempt number.
    /// </summary>
    public int Attempt { get; }

    /// <summary>
    /// Gets the delay suggested by the retry strategy.
    /// </summary>
    public TimeSpan DelayHint { get; }
}
