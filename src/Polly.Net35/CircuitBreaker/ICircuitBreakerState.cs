using System;

namespace Polly.CircuitBreaker
{
    /// <summary>
    /// Interface used to describe the needed functionality needed for the circuitbreaker pattern.
    /// </summary>
    internal interface ICircuitBreakerState
    {
        /// <summary>
        /// Last exception handled.
        /// </summary>
        Exception LastException { get; }

        /// <summary>
        /// Get current state of the circuit.
        /// </summary>
        /// <returns>bool that states wether the circuit is broken or not.</returns>
        bool IsBroken { get; }

        /// <summary>
        /// Reset the state of the circuitbreaker.
        /// </summary>
        void Reset();

        /// <summary>
        /// Used by Polly to try to break the circuit. If an exception is recieved that matches the policy, then Polly will call this method to try to break the state of the circuit.
        /// </summary>
        /// <param name="ex"></param>
        void TryBreak(Exception ex);
    }
}