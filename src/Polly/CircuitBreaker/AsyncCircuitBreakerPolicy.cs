using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.CircuitBreaker
{
    /// <summary>
    /// A circuit-breaker policy that can be applied to asynchronous executions.
    /// </summary>
    public class AsyncCircuitBreakerPolicy : AsyncPolicyV8, IAsyncCircuitBreakerPolicy
    {
        internal readonly ICircuitController<object> _breakerController;

        internal AsyncCircuitBreakerPolicy(
            PolicyBuilder policyBuilder, 
            ICircuitController<object> breakerController
            ) : base(policyBuilder)
        {
            _breakerController = breakerController;
        }

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
        public void Isolate() => _breakerController.Isolate();

        /// <summary>
        /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
        /// </summary>
        public void Reset() => _breakerController.Reset();

        /// <inheritdoc/>
        protected override async Task<TResult> AsyncGenericImplementationV8<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            TResult result = default;

            // This additional call in <object> is necessary, because _breakerController is generic in <object>.
            // We therefore have to convert the whole execution to object.
            // It could be removed if ICircuitController<TResult> was refactored to not be generic in TResult.
            // Which could be done by moving onBreak (and similar) out of ICircuitController<>, instead to parameters passed to CircuitBreakerEngineV8.Implementation<>() (as retry policy does)
            // and by removing the LastHandledResult property.
            // An allocation is introduced by the closure over action.
            await AsyncCircuitBreakerEngineV8.ImplementationAsync<IAsyncExecutable<object>, object>(
                new AsyncExecutableAction(async (ctx, ct, cap) => { result = await action.ExecuteAsync(ctx, ct, cap).ConfigureAwait(cap); }), 
                context,
                cancellationToken,
                continueOnCapturedContext,
                ExceptionPredicates,
                ResultPredicates<object>.None,
                _breakerController).ConfigureAwait(continueOnCapturedContext);

            return result;
        }
    }

    /// <summary>
    /// A circuit-breaker policy that can be applied to asynchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class AsyncCircuitBreakerPolicy<TResult> : AsyncPolicyV8<TResult>, IAsyncCircuitBreakerPolicy<TResult>
    {
        internal readonly ICircuitController<TResult> _breakerController;

        internal AsyncCircuitBreakerPolicy(
            PolicyBuilder<TResult> policyBuilder, 
            ICircuitController<TResult> breakerController
            ) : base(policyBuilder)
        {
            _breakerController = breakerController;
        }

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
        /// Gets the last result returned from a user delegate which the circuit-breaker handled.
        /// <remarks>This will be default(<typeparamref name="TResult"/>) if no results have been handled by the circuit-breaker since the circuit last closed, or if the last event handled by the circuit was an exception.</remarks>
        /// </summary>
        public TResult LastHandledResult => _breakerController.LastHandledResult;

        /// <summary>
        /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="Reset()"/> is made.
        /// </summary>
        public void Isolate() => _breakerController.Isolate();

        /// <summary>
        /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
        /// </summary>
        public void Reset() => _breakerController.Reset();

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override Task<TResult> AsyncGenericImplementationV8<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
            => AsyncCircuitBreakerEngineV8.ImplementationAsync(
                action,
                context,
                cancellationToken,
                continueOnCapturedContext,
                ExceptionPredicates,
                ResultPredicates,
                _breakerController);
    }
}
