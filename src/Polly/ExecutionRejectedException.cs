﻿using System;
#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace Polly
{
    /// <summary>
    /// Exception thrown when a policy rejects execution of a delegate.  
    /// <remarks>More specific exceptions which derive from this type, are generally thrown.</remarks>
    /// </summary>
    public abstract class ExecutionRejectedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionRejectedException"/> class.
        /// </summary>
        protected ExecutionRejectedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionRejectedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        protected ExecutionRejectedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionRejectedException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The inner exception.</param>
        protected ExecutionRejectedException(string message, Exception inner) : base(message, inner)
        {
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionRejectedException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected ExecutionRejectedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
