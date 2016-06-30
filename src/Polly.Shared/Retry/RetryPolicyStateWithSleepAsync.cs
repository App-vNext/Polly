#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleep<TResult> : IRetryPolicyState<TResult>
    {
        private readonly Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> _onRetryAsync;


        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task> onRetryAsync, Context context)
        {
            _onRetryAsync = onRetryAsync;
            _context = context;
            _sleepDurationsEnumerator = sleepDurations.GetEnumerator();
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync, Context context) :
            this(sleepDurations, (delegateResult, span, i, c) => onRetryAsync(delegateResult, span, c), context)
        {
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Func<DelegateResult<TResult>, TimeSpan, Task> onRetryAsync) :
            this(sleepDurations, (delegateResult, span, context) => onRetryAsync(delegateResult, span), Context.Empty)
        {
        }

        public async Task<bool> CanRetryAsync(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (!_sleepDurationsEnumerator.MoveNext()) return false;

            _errorCount += 1;

            var currentTimeSpan = _sleepDurationsEnumerator.Current;
            await _onRetryAsync(delegateResult, currentTimeSpan, _errorCount, _context).ConfigureAwait(continueOnCapturedContext);

            await SystemClock.SleepAsync(currentTimeSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            return true;
        }
    }
}

#endif