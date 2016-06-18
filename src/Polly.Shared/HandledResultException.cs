using System;
using System.Runtime.Serialization;

namespace Polly
{
    /// <summary>
    /// Exception to wrap a TResult which the given policy considers a fault.
    /// </summary>
    /// <typeparam name="TResult">The TResult type being handled by the policy.</typeparam>
#if !PORTABLE
    [Serializable]
#endif
    public class HandledResultException<TResult> : Exception
    {
        private readonly TResult result;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandledResultException{TResult}"/> class.
        /// </summary>
        public HandledResultException(TResult result)
        {
            this.result = result;
        }

        /// <summary>
        /// The result value which was considered a handled fault, by the policy.
        /// </summary>
        public TResult Result { get { return result; } }

#if !PORTABLE
        /// <summary>
        /// Initializes a new instance of the <see cref="HandledResultException{TResult}"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected HandledResultException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}