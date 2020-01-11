using System;
using System.Diagnostics;
using System.Threading;

namespace Polly.CircuitBreaker
{
    /// <summary>
    /// A circuit-breaker policy that can be applied to synchronous executions.
    /// </summary>
    public class CircuitBreakerPolicy : PolicyV8, ISyncCircuitBreakerPolicy
    {
        internal readonly ICircuitController<object> _breakerController;

        internal CircuitBreakerPolicy(
            PolicyBuilder policyBuilder,
            ICircuitController<object> breakerController
            ) : base(policyBuilder)
            => _breakerController = breakerController;

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
        [DebuggerStepThrough]
        protected override TResult SyncGenericImplementationV8<TExecutable, TResult>(in TExecutable action, Context context,
                CancellationToken cancellationToken)
            => ImplementationSyncV8Internal<TExecutable, TResult>(action, context, cancellationToken);

        // This additional private method is necessary, because _breakerController is generic in <object>.
        // We therefore have to convert the whole execution to object.
        // It could be removed if ICircuitController<TResult> was refactored to not be generic in TResult.
        // Which could be done by moving onBreak (and similar) out of ICircuitController<>, instead to parameters passed to CircuitBreakerEngineV8.Implementation<>() (as retry policy does)
        // and by removing the LastHandledResult property.
        // Further, we need to drop the 'in' parameter on TExecutable action, to use action in the lambda.  
        // An allocation is introduced by the closure over action.
        private TResult ImplementationSyncV8Internal<TExecutable, TResult>(TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>
        {
            TResult result = default;
            CircuitBreakerEngineV8.Implementation<ISyncExecutable<object>, object>(
                new SyncExecutableAction((ctx, ct) => result = action.Execute(ctx, ct)),
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates<object>.None,
                _breakerController);
            return result;
        }
    }

    /// <summary>
    /// A circuit-breaker policy that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public class CircuitBreakerPolicy<TResult> : PolicyV8<TResult>, ISyncCircuitBreakerPolicy<TResult>
    {
        internal readonly ICircuitController<TResult> _breakerController;

        internal CircuitBreakerPolicy(
            PolicyBuilder<TResult> policyBuilder, 
            ICircuitController<TResult> breakerController
            ) : base(policyBuilder)
            => _breakerController = breakerController;

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
        public void Isolate() => _breakerController.Isolate();

        /// <summary>
        /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
        /// </summary>
        public void Reset() => _breakerController.Reset();

        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override TResult SyncGenericImplementationV8<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            => CircuitBreakerEngineV8.Implementation(
                action,
                context,
                cancellationToken,
                ExceptionPredicates,
                ResultPredicates,
                _breakerController);
    }
}
