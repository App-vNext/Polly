namespace Polly.CircuitBreaker;

/// <summary>
/// Arguments used by <see cref="CircuitBreakerStrategyOptions{TResult}.OnHalfOpened"/> event.
/// </summary>
public sealed class OnCircuitHalfOpenedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnCircuitHalfOpenedArguments"/> class.
    /// </summary>
    /// <param name="context">The context instance.</param>
    public OnCircuitHalfOpenedArguments(ResilienceContext context) => Context = context;

    /// <summary>
    /// Gets the context associated with the execution of a user-provided callback.
    /// </summary>
    public ResilienceContext Context { get; }
}
