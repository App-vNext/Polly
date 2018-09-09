using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryStateWaitAndRetryWithProvider<TResult>
    {
        private readonly Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> _onRetryAsync;

        public RetryStateWaitAndRetryWithProvider(int retryCount, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync, Context context)
        {
            _retryCount = retryCount;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetryAsync = onRetryAsync;
            _context = context;
        }

        public async Task<bool> CanRetryAsync(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            _errorCount += 1;

            bool shouldRetry = _errorCount <= _retryCount;
            if (shouldRetry)
            {
                TimeSpan waitTimeSpan = _sleepDurationProvider(_errorCount, delegateResult, _context);

                await _onRetryAsync(delegateResult, waitTimeSpan, _errorCount, _context).ConfigureAwait(continueOnCapturedContext);

                await SystemClock.SleepAsync(waitTimeSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }

            return shouldRetry;
        }

    }
}
