using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial class RetryStateRetryWithCount<TResult> : IRetryPolicyState<TResult>
    {
        private readonly Func<DelegateResult<TResult>, int, Context, Task> _onRetryAsync;

        public RetryStateRetryWithCount(int retryCount, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync, Context context)
        {
            _retryCount = retryCount;
            _onRetryAsync = onRetryAsync;
            _context = context;
        }

        public async Task<bool> CanRetryAsync(DelegateResult<TResult> delegateResult, CancellationToken ct, bool continueOnCapturedContext)
        {
            _errorCount += 1;

            bool shouldRetry = _errorCount <= _retryCount;
            if (shouldRetry)
            {
               await _onRetryAsync(delegateResult, _errorCount, _context).ConfigureAwait(continueOnCapturedContext);
            }

            return shouldRetry;
        }
    }
}

