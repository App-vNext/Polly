namespace Polly.CircuitBreaker;

#pragma warning disable CA1062 // Validate arguments of public methods // Temporary stub

/// <summary>
/// A circuit-breaker policy that can be applied to async delegates.
/// </summary>
public class AsyncCircuitBreakerPolicy : AsyncPolicy, ICircuitBreakerPolicy
{
    internal readonly ICircuitController<EmptyStruct> BreakerController;

    internal AsyncCircuitBreakerPolicy(
        PolicyBuilder policyBuilder,
        ICircuitController<EmptyStruct> breakerController)
        : base(policyBuilder) =>
        BreakerController = breakerController;

    /// <summary>
    /// Gets the state of the underlying circuit.
    /// </summary>
    public CircuitState CircuitState => BreakerController.CircuitState;

    /// <summary>
    /// Gets the last exception handled by the circuit-breaker.
    /// <remarks>This will be null if no exceptions have been handled by the circuit-breaker since the circuit last closed.</remarks>
    /// </summary>
    public Exception LastException => BreakerController.LastException;

    /// <summary>
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="Reset()"/> is made.
    /// </summary>
    public void Isolate() =>
        BreakerController.Isolate();

    /// <summary>
    /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
    /// </summary>
    public void Reset() =>
        BreakerController.Reset();

    /// <inheritdoc/>
    protected override async Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        TResult result = default;
        await AsyncCircuitBreakerEngine.ImplementationAsync<EmptyStruct>(
            async (ctx, ct) => { result = await action(ctx, ct).ConfigureAwait(continueOnCapturedContext); return EmptyStruct.Instance; },
            context,
            continueOnCapturedContext,
            ExceptionPredicates,
            ResultPredicates<EmptyStruct>.None,
            BreakerController,
            cancellationToken).ConfigureAwait(continueOnCapturedContext);
        return result;
    }
}

/// <summary>
/// A circuit-breaker policy that can be applied to async delegates.
/// </summary>
/// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
public class AsyncCircuitBreakerPolicy<TResult> : AsyncPolicy<TResult>, ICircuitBreakerPolicy<TResult>
{
    internal readonly ICircuitController<TResult> BreakerController;

    internal AsyncCircuitBreakerPolicy(
        PolicyBuilder<TResult> policyBuilder,
        ICircuitController<TResult> breakerController)
        : base(policyBuilder) =>
        BreakerController = breakerController;

    /// <summary>
    /// Gets the state of the underlying circuit.
    /// </summary>
    public CircuitState CircuitState => BreakerController.CircuitState;

    /// <summary>
    /// Gets the last exception handled by the circuit-breaker.
    /// <remarks>This will be null if no exceptions have been handled by the circuit-breaker since the circuit last closed.</remarks>
    /// </summary>
    public Exception LastException => BreakerController.LastException;

    /// <summary>
    /// Gets the last result returned from a user delegate which the circuit-breaker handled.
    /// <remarks>This will be default(<typeparamref name="TResult"/>) if no results have been handled by the circuit-breaker since the circuit last closed, or if the last event handled by the circuit was an exception.</remarks>
    /// </summary>
    public TResult LastHandledResult => BreakerController.LastHandledResult;

    /// <summary>
    /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="Reset()"/> is made.
    /// </summary>
    public void Isolate() =>
        BreakerController.Isolate();

    /// <summary>
    /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
    /// </summary>
    public void Reset() =>
        BreakerController.Reset();

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncCircuitBreakerEngine.ImplementationAsync(
            action,
            context,
            continueOnCapturedContext,
            ExceptionPredicates,
            ResultPredicates,
            BreakerController,
            cancellationToken);
}
