namespace Polly.Simmy.Outcomes;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the outcome chaos strategy to notify that an outcome was injected.
/// </summary>
public readonly struct OutcomeGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutcomeGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    public OutcomeGeneratorArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext Context { get; }
}
