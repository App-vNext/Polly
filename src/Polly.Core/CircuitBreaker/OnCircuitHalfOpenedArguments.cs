namespace Polly.CircuitBreaker;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnHalfOpened"/> event.
/// </summary>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct OnCircuitHalfOpenedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnCircuitHalfOpenedArguments"/> struct.
    /// </summary>
    /// <param name="context">The context instance.</param>
    public OnCircuitHalfOpenedArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the context associated with the execution of a user-provided callback.
    /// </summary>
    public ResilienceContext Context { get; }
}
