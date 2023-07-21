namespace Polly.Timeout;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the timeout strategy to retrieve a timeout for current execution.
/// </summary>
public readonly struct TimeoutGeneratorArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutGeneratorArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    public TimeoutGeneratorArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the context associated with the execution of a user-provided callback.
    /// </summary>
    public ResilienceContext Context { get; }
}

