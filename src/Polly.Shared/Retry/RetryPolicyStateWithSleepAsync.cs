#if SUPPORTS_ASYNC

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleep : IRetryPolicyState
    {
        private readonly Func<Exception, TimeSpan, Context, Task> _onRetryAsync;


        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, Context, Task> onRetryAsync, Context context)
        {
            _onRetryAsync = onRetryAsync;
            _context = context;
            _sleepDurationsEnumerator = sleepDurations.GetEnumerator();
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Func<Exception, TimeSpan, Task> onRetryAsync) :
            this(sleepDurations, (exception, span, context) => onRetryAsync(exception, span), Context.Empty)
        {
        }

        public async Task<bool> CanRetryAsync(Exception ex, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (!_sleepDurationsEnumerator.MoveNext()) return false;

            _errorCount += 1;

            var currentTimeSpan = _sleepDurationsEnumerator.Current;
            await _onRetryAsync(ex, currentTimeSpan, _context).ConfigureAwait(continueOnCapturedContext);

            await SystemClock.SleepAsync(currentTimeSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            return true;
        }
    }
}

#endif