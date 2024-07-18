namespace Polly.CircuitBreaker;

/// <summary>
/// Defines properties and methods common to all circuit-breaker policies.
/// </summary>
public interface ICircuitBreakerPolicy : IsPolicy
{
    /// <summary>
    /// Gets the state of the underlying circuit.
    /// </summary>
    CircuitState CircuitState { get; }

    /// <summary>
    /// Gets the last exception handled by the circuit-breaker.
    /// <remarks>This will be null if no exceptions have been handled by the circuit-breaker since the circuit last closed.</remarks>
    /// </summary>
    Exception LastException { get; }

    /// <summary>
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="CircuitBreakerPolicy.Reset"/> is made.
    /// </summary>
    void Isolate();

    /// <summary>
    /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
    /// </summary>
    void Reset();
}

/// <summary>
/// Defines properties and methods common to all circuit-breaker policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
#pragma warning disable S3246
public interface ICircuitBreakerPolicy<TResult> : ICircuitBreakerPolicy
#pragma warning restore S3246
{
    /// <summary>
    /// Gets the last result returned from a user delegate which the circuit-breaker handled.
    /// <remarks>This will be default(<typeparamref name="TResult"/>) if no results have been handled by the circuit-breaker since the circuit last closed, or if the last event handled by the circuit was an exception.</remarks>
    /// </summary>
    TResult LastHandledResult { get; }
}
