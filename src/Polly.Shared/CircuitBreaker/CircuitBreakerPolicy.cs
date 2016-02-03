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
        /// Gets the last exception handled by the circuit-breaker.
        /// </summary>
        public Exception LastException {
            get { return _breakerState.LastException; }
        }

        /// <summary>
        /// Returns whether the circuit breaker is currently in a broken (open) state.
        /// </summary>
        public bool IsBroken {
            get { return _breakerState.IsBroken; }
        }

        /// <summary>
        /// Closes the circuit, and resets any statistics controlling automatic circuit-breaking.
        /// </summary>
        public void Reset()
        {
            _breakerState.Reset();
        }
    }
}
