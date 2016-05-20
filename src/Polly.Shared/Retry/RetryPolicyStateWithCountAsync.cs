#if SUPPORTS_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithCount : IRetryPolicyState
    {
        private readonly Func<Exception, int, Context, Task> _onRetryAsync;

        public RetryPolicyStateWithCount(int retryCount, Func<Exception, int, Context, Task> onRetryAsync, Context context)
        {
            _retryCount = retryCount;
            _onRetryAsync = onRetryAsync;
            _context = context;
        }

        public RetryPolicyStateWithCount(int retryCount, Func<Exception, int, Task> onRetryAsync) :
            this(retryCount, (exception, i, context) => onRetryAsync(exception, i), Context.Empty)
        {
        }

        public async Task<bool> CanRetryAsync(Exception ex, CancellationToken ct, bool continueOnCapturedContext)
        {
            _errorCount += 1;

            var shouldRetry = _errorCount <= _retryCount;
            if (shouldRetry)
            {
               await _onRetryAsync(ex, _errorCount, _context).ConfigureAwait(continueOnCapturedContext);
            }

            return shouldRetry;
        }
    }
}

#endif