using System;
using Polly.CircuitBreaker;

namespace Polly.Shared.CircuitBreaker
{
    /// <summary>
    /// Common interface for CB policies allowing users of the library to 
    /// </summary>
    public interface ICircuitBreakerPolicy
    {
        /// <summary>
        /// Gets the state of the underlying circuit.
        /// </summary>
        CircuitState CircuitState { get; }

        /// <summary>
        /// Gets the last exception handled by the circuit-breaker.
        /// </summary>
        Exception LastException { get; }

        /// <summary>
        /// Isolates (opens) the circuit manually, and holds it in this state until a call to <see cref="Reset()"/> is made.
        /// </summary>
        void Isolate();

        /// <summary>
        /// Closes the circuit, and resets any statistics controlling automated circuit-breaking.
        /// </summary>
        void Reset();
    }
}
