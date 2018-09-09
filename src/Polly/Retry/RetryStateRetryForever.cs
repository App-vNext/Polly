using System;
using System.Threading;

namespace Polly.Retry
{
    internal partial class RetryStateRetryForever<TResult> : IRetryPolicyState<TResult>
    {
        private readonly Action<DelegateResult<TResult>, Context> _onRetry;
        private readonly Context _context;

        public RetryStateRetryForever(Action<DelegateResult<TResult>, Context> onRetry, Context context)
        {
            _onRetry = onRetry;
            _context = context;
        }

        public bool CanRetry(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken)
        {
            _onRetry(delegateResult, _context);
            return true;
        }
    }
}