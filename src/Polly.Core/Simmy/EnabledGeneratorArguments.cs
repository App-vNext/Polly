namespace Polly.Simmy;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Defines the arguments for the <see cref="ChaosStrategyOptions{TResult}.EnabledGenerator"/>.
/// </summary>
public readonly struct EnabledGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnabledGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="context">The resilience context instance.</param>
    public EnabledGeneratorArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext Context { get; }
}
