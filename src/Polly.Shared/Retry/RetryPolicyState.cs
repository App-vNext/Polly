using System;

namespace Polly.Retry
{
    internal partial class RetryPolicyState<TResult> : IRetryPolicyState<TResult>
    {
        private readonly Action<DelegateResult<TResult>, Context> _onRetry;
        private readonly Context _context;

        public RetryPolicyState(Action<DelegateResult<TResult>, Context> onRetry, Context context)
        {
            _onRetry = onRetry;
            _context = context;
        }

        public RetryPolicyState(Action<DelegateResult<TResult>> onRetry) :
            this((delegateResult, context) => onRetry(delegateResult), Context.Empty)
        {
        }

        public bool CanRetry(DelegateResult<TResult> delegateResult)
        {
            _onRetry(delegateResult, _context);
            return true;
        }
    }
}