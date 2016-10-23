using System;
#if !PORTABLE
using System.Runtime.Serialization;
#endif

namespace Polly.CircuitBreaker
{
    /// <summary>
    /// Exception thrown when a circuit is broken.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public class BrokenCircuitException : ExecutionRejectedException
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
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The inner exception.</param>
        public BrokenCircuitException(string message, Exception inner) : base(message, inner)
        {
        }

#if !PORTABLE
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
#endif
    }

    /// <summary>
    /// Exception thrown when a circuit is broken.
    /// </summary>
    /// <typeparam name="TResult">The type of returned results being handled by the policy.</typeparam>
#if !PORTABLE
    [Serializable]
#endif
    public class BrokenCircuitException<TResult> : BrokenCircuitException
    {
        private readonly TResult result;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokenCircuitException{TResult}"/> class.
        /// </summary>
        /// <param name="result">The result which caused the circuit to break.</param>
        public BrokenCircuitException(TResult result) : base()
        {
            this.result = result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokenCircuitException{TResult}"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="result">The result which caused the circuit to break.</param>
        public BrokenCircuitException(string message, TResult result) : base(message)
        {
            this.result = result;
        }

        /// <summary>
        /// The result value which was considered a handled fault, by the policy.
        /// </summary>
        public TResult Result { get { return result; } }

#if !PORTABLE
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
#endif
    }
}