using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.CircuitBreaker
{
    /// <summary>
    /// A circuit-breaker policy that can be applied to delegates.
    /// </summary>
    public partial class CircuitBreakerPolicy : ContextualPolicy
    {
        private readonly ICircuitBreakerState _breakerState;

        internal CircuitBreakerPolicy(Action<Action, Context> exceptionPolicy, IEnumerable<ExceptionPredicate> exceptionPredicates, ICircuitBreakerState breakerState) 
            : base(exceptionPolicy, exceptionPredicates)
        {
            _breakerState = breakerState;
        }

        /// <summary>
        /// Gets the state of the underlying circuit.
        /// </summary>
        public CircuitState CircuitState
        {
            get { return _breakerState.CircuitState; }
        }

        /// <summary>
        /// Gets the last exception handled by the circuit-breaker.
        /// </summary>
        public Exception LastException {
            get { return _breakerState.LastException; }
        }

        /// <summary>
        /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="Reset()"/> is made.
        /// </summary>
        public void Isolate()
        {
            _breakerState.Isolate();
        }

        /// <summary>
        /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
        /// </summary>
        public void Reset()
        {
            _breakerState.Reset();
        }
    }
}
