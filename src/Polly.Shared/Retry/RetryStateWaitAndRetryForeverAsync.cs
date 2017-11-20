using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryStateWaitAndRetryForever<TResult> : IRetryPolicyState<TResult>
    {
        private readonly Func<DelegateResult<TResult>, TimeSpan, Context, Task> _onRetryAsync;

        public RetryStateWaitAndRetryForever(Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync, Context context)
        {
            _sleepDurationProvider = sleepDurationProvider;
            _onRetryAsync = onRetryAsync;
            _context = context;
        }
        
        public async Task<bool> CanRetryAsync(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }

            TimeSpan waitTimeSpan = _sleepDurationProvider(_errorCount, delegateResult, _context);

            await _onRetryAsync(delegateResult, waitTimeSpan, _context).ConfigureAwait(continueOnCapturedContext);

            await SystemClock.SleepAsync(waitTimeSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            return true;
        }        
    }
}

