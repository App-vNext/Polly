namespace Polly.Simmy.Outcomes;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="ChaosOutcomeStrategyOptions{TResult}.OutcomeGenerator"/>.
/// </summary>
public readonly struct OutcomeGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutcomeGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="context">The resilience context instance.</param>
    public OutcomeGeneratorArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the resilience context instance.
    /// </summary>
    public ResilienceContext Context { get; }
}
