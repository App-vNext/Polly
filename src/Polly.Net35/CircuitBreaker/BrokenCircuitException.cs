using System;
using System.Runtime.Serialization;

namespace Polly.CircuitBreaker
{
    /// <summary>
    /// Exception thrown when a circuit is broken.
    /// </summary>
    [Serializable]
    public class BrokenCircuitException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrokenCircuitException"/> class.
        /// </summary>
        public BrokenCircuitException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokenCircuitException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BrokenCircuitException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokenCircuitException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public BrokenCircuitException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokenCircuitException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected BrokenCircuitException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}