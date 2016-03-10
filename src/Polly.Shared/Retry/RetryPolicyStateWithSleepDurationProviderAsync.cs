#if SUPPORTS_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleepDurationProvider : IRetryPolicyState
    {
        public async Task<bool> CanRetryAsync(Exception ex, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }

            var currentTimeSpan = _sleepDurationProvider(_errorCount);
            _onRetry(ex, currentTimeSpan, _context);

            await SystemClock.SleepAsync(currentTimeSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            return true;
        }        
    }
}

#endif