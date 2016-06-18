using System;

namespace Polly
{
    /// <summary>
    /// The captured outcome of executing an individual Func&lt;TResult&gt;
    /// </summary>
    internal class DelegateOutcome<TResult>
    {
        private readonly TResult _result;// = default (TResult);
        private readonly Exception _exception;// = null;

        internal DelegateOutcome(TResult result)
        {
            _result = result;
        }

        internal DelegateOutcome(Exception exception)
        {
            _exception = exception;
        }

        public TResult Result
        {
            get { return _result; }
        }

        public Exception Exception
        {
            get { return _exception; }
        }
    }
}
