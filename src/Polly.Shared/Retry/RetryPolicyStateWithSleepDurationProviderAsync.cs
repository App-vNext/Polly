#if SUPPORTS_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleepDurationProvider : IRetryPolicyState
    {
        private readonly Func<Exception, TimeSpan, Context, Task> _onRetryAsync;

        public RetryPolicyStateWithSleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Context, Task> onRetryAsync, Context context)
        {
            this._sleepDurationProvider = sleepDurationProvider;
            _onRetryAsync = onRetryAsync;
            _context = context;
        }

        public RetryPolicyStateWithSleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider, Func<Exception, TimeSpan, Task> onRetryAsync) :
            this(sleepDurationProvider, (exception, timespan, context) => onRetryAsync(exception, timespan), Context.Empty)
        {
        }
        
        public async Task<bool> CanRetryAsync(Exception ex, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }

            var currentTimeSpan = _sleepDurationProvider(_errorCount);
            await _onRetryAsync(ex, currentTimeSpan, _context).ConfigureAwait(continueOnCapturedContext);

            await SystemClock.SleepAsync(currentTimeSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            return true;
        }        
    }
}

#endif