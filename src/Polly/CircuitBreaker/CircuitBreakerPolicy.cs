namespace Polly.CircuitBreaker;

/// <summary>
/// A circuit-breaker policy that can be applied to delegates.
/// </summary>
public class CircuitBreakerPolicy : Policy, ICircuitBreakerPolicy
{
    internal readonly ICircuitController<EmptyStruct> _breakerController;

    internal CircuitBreakerPolicy(
        PolicyBuilder policyBuilder,
        ICircuitController<EmptyStruct> breakerController)
        : base(policyBuilder) =>
        _breakerController = breakerController;

    /// <summary>
    /// Gets the state of the underlying circuit.
    /// </summary>
    public CircuitState CircuitState => _breakerController.CircuitState;

    /// <summary>
    /// Gets the last exception handled by the circuit-breaker.
    /// <remarks>This will be null if no exceptions have been handled by the circuit-breaker since the circuit last closed.</remarks>
    /// </summary>
    public Exception LastException => _breakerController.LastException;

    /// <summary>
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="Reset()"/> is made.
    /// </summary>
    public void Isolate() =>
        _breakerController.Isolate();

    /// <summary>
    /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
    /// </summary>
    public void Reset() =>
        _breakerController.Reset();

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        TResult result = default;
        CircuitBreakerEngine.Implementation<EmptyStruct>(
            (ctx, ct) => { result = action(ctx, ct); return EmptyStruct.Instance; },
            context,
            cancellationToken,
            ExceptionPredicates,
            ResultPredicates<EmptyStruct>.None,
            _breakerController);
        return result;
    }
}

/// <summary>
/// A circuit-breaker policy that can be applied to delegates returning a value of type <typeparamref name="TResult"/>.
/// </summary>
public class CircuitBreakerPolicy<TResult> : Policy<TResult>, ICircuitBreakerPolicy<TResult>
{
    internal readonly ICircuitController<TResult> _breakerController;

    internal CircuitBreakerPolicy(
        PolicyBuilder<TResult> policyBuilder,
        ICircuitController<TResult> breakerController)
        : base(policyBuilder) =>
        _breakerController = breakerController;

    /// <summary>
    /// Gets the state of the underlying circuit.
    /// </summary>
    public CircuitState CircuitState => _breakerController.CircuitState;

    /// <summary>
    /// Gets the last exception handled by the circuit-breaker.
    /// <remarks>This will be null if no exceptions have been handled by the circuit-breaker since the circuit last closed, or if the last event handled by the circuit was a handled <typeparamref name="TResult"/> value.</remarks>
    /// </summary>
    public Exception LastException => _breakerController.LastException;

    /// <summary>
    /// Gets the last result returned from a user delegate which the circuit-breaker handled.
    /// <remarks>This will be default(<typeparamref name="TResult"/>) if no results have been handled by the circuit-breaker since the circuit last closed, or if the last event handled by the circuit was an exception.</remarks>
    /// </summary>
    public TResult LastHandledResult => _breakerController.LastHandledResult;

    /// <summary>
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="Reset()"/> is made.
    /// </summary>
    public void Isolate() =>
        _breakerController.Isolate();

    /// <summary>
    /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
    /// </summary>
    public void Reset() =>
        _breakerController.Reset();

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        CircuitBreakerEngine.Implementation(
            action,
            context,
            cancellationToken,
            ExceptionPredicates,
            ResultPredicates,
            _breakerController);
}
