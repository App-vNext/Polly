using System;
#if !PORTABLE
using System.Runtime.Serialization;
#endif

namespace Polly.CircuitBreaker
{
    /// <summary>
    /// Exception thrown when a circuit is isolated (held open) by manual override.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public class IsolatedCircuitException : BrokenCircuitException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsolatedCircuitException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public IsolatedCircuitException(string message) : base(message) { }

#if !PORTABLE
        /// <summary>
        /// Initializes a new instance of the <see cref="IsolatedCircuitException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected IsolatedCircuitException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
