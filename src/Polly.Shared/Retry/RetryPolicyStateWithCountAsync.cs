#if SUPPORTS_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithCount<TResult> : IRetryPolicyState<TResult>
    {
        private readonly Func<DelegateResult<TResult>, int, Context, Task> _onRetryAsync;

        public RetryPolicyStateWithCount(int retryCount, Func<DelegateResult<TResult>, int, Context, Task> onRetryAsync, Context context)
        {
            _retryCount = retryCount;
            _onRetryAsync = onRetryAsync;
            _context = context;
        }

        public RetryPolicyStateWithCount(int retryCount, Func<DelegateResult<TResult>, int, Task> onRetryAsync) :
            this(retryCount, (delegateResult, i, context) => onRetryAsync(delegateResult, i), Context.Empty)
        {
        }

        public async Task<bool> CanRetryAsync(DelegateResult<TResult> delegateResult, CancellationToken ct, bool continueOnCapturedContext)
        {
            _errorCount += 1;

            var shouldRetry = _errorCount <= _retryCount;
            if (shouldRetry)
            {
               await _onRetryAsync(delegateResult, _errorCount, _context).ConfigureAwait(continueOnCapturedContext);
            }

            return shouldRetry;
        }
    }
}

#endif