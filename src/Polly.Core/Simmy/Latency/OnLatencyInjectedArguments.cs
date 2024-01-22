namespace Polly.Simmy.Latency;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the latency chaos strategy to notify that a delayed occurred.
/// </summary>
public readonly struct OnLatencyInjectedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnLatencyInjectedArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    /// <param name="latency">The latency that was injected.</param>
    public OnLatencyInjectedArguments(ResilienceContext context, TimeSpan latency)
    {
        Context = context;
        Latency = latency;
    }

    /// <summary>
    /// Gets the ResilienceContext instance.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the latency that was injected.
    /// </summary>
    public TimeSpan Latency { get; }
}
