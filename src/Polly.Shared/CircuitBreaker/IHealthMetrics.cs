using Polly.CircuitBreaker;

namespace Polly.CircuitBreaker
{
    /// <summary>
    /// Describes the interface for the <see cref="TimesliceCircuitController"/> on how to calculate
    /// the statistical information on the health of the connection.
    /// </summary>
    interface IHealthMetrics
    {
        /// <summary>
        /// Increments the success statistics. 
        /// Is called when a call through the circuit has gone well.
        /// </summary>
        void IncrementSuccess_NeedsLock();

        /// <summary>
        /// Increments the failure statistics.
        /// Is called when a call through the circuit has failed with an exception handled by the circuit.
        /// </summary>
        void IncrementFailure_NeedsLock();

        /// <summary>
        /// Resets the statistics.
        /// </summary>
        void Reset_NeedsLock();

        /// <summary>
        /// Returns the health statistics on the circuit.
        /// </summary>
        /// <returns>Health statistics</returns>
        HealthCount GetHealthCount_NeedsLock();
    }
}
