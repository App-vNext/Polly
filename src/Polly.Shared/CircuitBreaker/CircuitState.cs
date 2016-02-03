using System;
using System.Collections.Generic;
using System.Text;
using Polly.CircuitBreaker;

namespace Polly.CircuitBreaker
{
    /// <summary>
    /// Describes the possible states the circuit of a CircuitBreaker may be in.
    /// </summary>
    public enum CircuitState
    {
        /// <summary>
        /// Closed - When the circuit is closed.  Execution of actions is allowed.
        /// </summary>
        Closed,
        /// <summary>
        /// Open - When the automated controller has opened the circuit (typically due to some failure threshold being exceeded by recent actions). Execution of actions is blocked.
        /// </summary>
        Open,
        /// <summary>
        /// Half-open - When the circuit is half-open, it is recovering from an open state.  The duration of break of the preceding open state has typically passed.  In the half-open state, actions may be executed, but the results of these actions may be treated with criteria different to normal operation, to decide if the circuit has recovered sufficiently to be placed back in to the closed state, or if continuing failures mean the circuit should revert to open perhaps more quickly than in normal operation.
        /// </summary>
        HalfOpen,
        /// <summary>
        /// Isolated - When the circuit has been placed into a fixed open state by a call to <see cref="CircuitBreakerPolicy.Isolate()"/>.  This isolates the circuit manually, blocking execution of all actions until a call to <see cref="CircuitBreakerPolicy.Reset()"/> is made.
        /// </summary>
        Isolated
    }
}
