#if SUPPORTS_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleepDurationProvider<TResult> : IRetryPolicyState<TResult>
    {
        private readonly Func<DelegateResult<TResult>, TimeSpan, Context, Task> _onRetryAsync;

        public RetryPolicyStateWithSleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync, Context context)
        {
            this._sleepDurationProvider = sleepDurationProvider;
            _onRetryAsync = onRetryAsync;
            _context = context;
        }

        public RetryPolicyStateWithSleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync) :
            this(sleepDurationProvider, (delegateResult, timespan, context) => onRetryAsync(delegateResult, timespan), Context.Empty)
        {
        }
        
        public async Task<bool> CanRetryAsync(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }

            var currentTimeSpan = _sleepDurationProvider(_errorCount);
            await _onRetryAsync(delegateResult, currentTimeSpan, _context).ConfigureAwait(continueOnCapturedContext);

            await SystemClock.SleepAsync(currentTimeSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            return true;
        }        
    }
}

#endif