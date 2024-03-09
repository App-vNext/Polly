namespace Polly.CircuitBreaker;

/// <summary>
/// Interface for controlling a circuit breaker.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
internal interface ICircuitController<TResult>
{
    /// <summary>
    /// Gets the state of the circuit.
    /// </summary>
    CircuitState CircuitState { get; }

    /// <summary>
    /// Gets the last exception that was handled by the circuit breaker.
    /// </summary>
    Exception LastException { get; }

    /// <summary>
    /// Gets the last result that was handled by the circuit breaker.
    /// </summary>
    TResult LastHandledResult { get; }

    /// <summary>
    /// Isolates the circuit breaker.
    /// </summary>
    void Isolate();

    /// <summary>
    /// Resets the circuit breaker.
    /// </summary>
    void Reset();

    /// <summary>
    /// Handles the circuit reset event.
    /// </summary>
    /// <param name="context">The context.</param>
    void OnCircuitReset(Context context);

    /// <summary>
    /// Handles the action pre-execute event.
    /// </summary>
    void OnActionPreExecute();

    /// <summary>
    /// Handles the action success event.
    /// </summary>
    /// <param name="context">The context.</param>
    void OnActionSuccess(Context context);

    /// <summary>
    /// Handles the action failure event.
    /// </summary>
    /// <param name="outcome">The outcome of the delegate.</param>
    /// <param name="context">The context.</param>
    void OnActionFailure(DelegateResult<TResult> outcome, Context context);
}
