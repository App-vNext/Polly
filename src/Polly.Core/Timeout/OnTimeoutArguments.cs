namespace Polly.Timeout;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the timeout strategy to notify that a timeout occurred.
/// </summary>
public readonly struct OnTimeoutArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnTimeoutArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    /// <param name="timeout">The timeout value assigned.</param>
    public OnTimeoutArguments(ResilienceContext context, TimeSpan timeout)
    {
        Context = context;
        Timeout = timeout;
    }

    /// <summary>
    /// Gets the context associated with the execution of a user-provided callback.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the timeout value assigned.
    /// </summary>
    public TimeSpan Timeout { get; }
}
