using System;
using System.Threading;

namespace Polly.Retry
{
    internal partial class RetryStateRetryWithCount<TResult> : IRetryPolicyState<TResult>
    {
        private int _errorCount;
        private readonly int _retryCount;
        private readonly Action<DelegateResult<TResult>, int, Context> _onRetry;
        private readonly Context _context;

        public RetryStateRetryWithCount(int retryCount, Action<DelegateResult<TResult>, int, Context> onRetry, Context context)
        {
            _retryCount = retryCount;
            _onRetry = onRetry;
            _context = context;
        }

        public bool CanRetry(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken)
        {
            _errorCount += 1;

            bool shouldRetry = _errorCount <= _retryCount;
            if (shouldRetry)
            {
                _onRetry(delegateResult, _errorCount, _context);
            }

            return shouldRetry;
        }
    }
}