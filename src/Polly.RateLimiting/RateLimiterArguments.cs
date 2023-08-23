namespace Polly.RateLimiting;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// The arguments used by the <see cref="RateLimiterStrategyOptions.RateLimiter"/> delegate.
/// </summary>
public readonly struct RateLimiterArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterArguments"/> struct.
    /// </summary>
    /// <param name="context">Context associated with the execution of a user-provided callback.</param>
    public RateLimiterArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the context associated with the execution of a user-provided callback.
    /// </summary>
    public ResilienceContext Context { get; }
}
