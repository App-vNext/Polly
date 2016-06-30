using System;

namespace Polly
{
    /// <summary>
    /// The captured outcome of executing an individual Func&lt;TResult&gt;
    /// </summary>
    public class DelegateResult<TResult>
    {
        private readonly TResult _result;
        private readonly Exception _exception;

        internal DelegateResult(TResult result)
        {
            _result = result;
        }

        internal DelegateResult(Exception exception)
        {
            _exception = exception;
        }

        /// <summary>
        /// The result of executing the delegate. Will be default(TResult) if an exception was thrown.
        /// </summary>
        public TResult Result
        {
            get { return _result; }
        }

        /// <summary>
        /// Any exception thrown while executing the delegate. Will be null if policy executed without exception.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }
    }
}
