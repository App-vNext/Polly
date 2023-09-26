namespace Polly.Simmy.Latency;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the latency chaos strategy to notify that a delayed occurred.
/// </summary>
internal readonly struct LatencyGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LatencyGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    public LatencyGeneratorArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext Context { get; }
}
