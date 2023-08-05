namespace Polly.Hedging;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by hedging delay generator.
/// </summary>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct HedgingDelayArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HedgingDelayArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    /// <param name="attemptNumber">The zero-based hedging attempt number.</param>
    public HedgingDelayArguments(ResilienceContext context, int attemptNumber)
    {
        Context = context;
        AttemptNumber = attemptNumber;
    }

    /// <summary>
    /// Gets the context associated with the execution of a user-provided callback.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the zero-based hedging attempt number.
    /// </summary>
    public int AttemptNumber { get; }
}
