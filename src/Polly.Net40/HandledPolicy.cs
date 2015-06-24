using System;

namespace Polly
{

    /// <summary>
    /// 
    /// </summary>
    public class HandledPolicy
    {
        /// <summary>
        /// 
        /// </summary>
        public Exception InnerException { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerException"></param>
        public HandledPolicy(Exception innerException)
        {
            InnerException = innerException;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class HandledPolicy<TResult>
    {
        /// <summary>
        /// 
        /// </summary>
        public TResult Result { get; }

        /// <summary>
        /// 
        /// </summary>
        public Exception InnerException { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public HandledPolicy(TResult result)
        {
            this.Result = result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerException"></param>
        public HandledPolicy(Exception innerException)
        {
            InnerException = innerException;
        }
    }
}