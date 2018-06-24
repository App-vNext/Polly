using System;
using System.Threading;

namespace Polly.Retry
{
    internal partial class RetryStateRetryForeverWithCount<TResult> : IRetryPolicyState<TResult>
    {
        private int _errorCount;
        private readonly Action<DelegateResult<TResult>, int, Context> _onRetry;
        private readonly Context _context;

        public RetryStateRetryForeverWithCount(Action<DelegateResult<TResult>, int, Context> onRetry, Context context)
        {
            _onRetry = onRetry;
            _context = context;
        }

        public bool CanRetry(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken)
        {
            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }

            _onRetry(delegateResult, _errorCount, _context);
            return true;
        }
    }
}