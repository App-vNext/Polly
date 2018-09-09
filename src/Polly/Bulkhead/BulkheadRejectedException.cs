﻿using System;
#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace Polly.Bulkhead
{
    /// <summary>
    /// Exception thrown when a bulkhead's semaphore and queue are full.
    /// </summary>
#if NETSTANDARD2_0
    [Serializable]
#endif
    public class BulkheadRejectedException : ExecutionRejectedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkheadRejectedException" /> class.
        /// </summary>
        public BulkheadRejectedException() : this("The bulkhead semaphore and queue are full and execution was rejected.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkheadRejectedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public BulkheadRejectedException(String message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkheadRejectedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BulkheadRejectedException(String message, Exception innerException) : base(message, innerException)
        {
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkheadRejectedException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected BulkheadRejectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
