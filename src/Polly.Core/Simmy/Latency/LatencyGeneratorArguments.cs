namespace Polly.Simmy.Latency;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the <see cref="ChaosLatencyStrategyOptions.LatencyGenerator"/>.
/// </summary>
public readonly struct LatencyGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LatencyGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="context">The resilience context instance.</param>
    public LatencyGeneratorArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the resilience context instance.
    /// </summary>
    public ResilienceContext Context { get; }
}
