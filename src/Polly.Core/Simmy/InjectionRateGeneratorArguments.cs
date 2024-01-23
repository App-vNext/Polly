namespace Polly.Simmy;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Defines the arguments for the <see cref="ChaosStrategyOptions.InjectionRateGenerator"/>.
/// </summary>
public readonly struct InjectionRateGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InjectionRateGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="context">The resilience context instance.</param>
    public InjectionRateGeneratorArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext Context { get; }
}
