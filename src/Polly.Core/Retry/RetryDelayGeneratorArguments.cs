namespace Polly.Retry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents the arguments used by <see cref="RetryStrategyOptions{TResult}.DelayGenerator"/> for generating the next retry delay.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct RetryDelayGeneratorArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryDelayGeneratorArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="outcome">The context in which the resilience operation or event occurred.</param>
    /// <param name="context">The outcome of the resilience operation or event.</param>
    /// <param name="attemptNumber">The zero-based attempt number.</param>
    public RetryDelayGeneratorArguments(ResilienceContext context, Outcome<TResult> outcome, int attemptNumber)
    {
        Context = context;
        Outcome = outcome;
        AttemptNumber = attemptNumber;
    }

    /// <summary>
    /// Gets the outcome of the resilience operation or event.
    /// </summary>
    public Outcome<TResult> Outcome { get; }

    /// <summary>
    /// Gets the context in which the resilience operation or event occurred.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets The zero-based attempt number.
    /// </summary>
    public int AttemptNumber { get; }
}
