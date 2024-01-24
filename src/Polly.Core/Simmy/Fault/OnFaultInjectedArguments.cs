namespace Polly.Simmy.Fault;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the fault chaos strategy to notify that an fault was injected.
/// </summary>
public readonly struct OnFaultInjectedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnFaultInjectedArguments"/> struct.
    /// </summary>
    /// <param name="context">The resilience context instance.</param>
    /// <param name="fault">The fault that was injected.</param>
    public OnFaultInjectedArguments(ResilienceContext context, Exception fault)
    {
        Context = context;
        Fault = fault;
    }

    /// <summary>
    /// Gets the resilience context instance.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the fault that was injected.
    /// </summary>
    public Exception Fault { get; }
}
