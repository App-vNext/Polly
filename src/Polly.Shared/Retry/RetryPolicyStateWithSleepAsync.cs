#if SUPPORTS_ASYNC

using System;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleep : IRetryPolicyState
    {
        static readonly Task<bool> Done = Task.FromResult(true);

        public async Task<bool> CanRetryAsync(Exception ex)
        {
            if (!_sleepDurationsEnumerator.MoveNext()) return false;

            var currentTimeSpan = _sleepDurationsEnumerator.Current;
            _onRetry(ex, currentTimeSpan, _context);

            await SystemClock.SleepAsync(currentTimeSpan);

            return true;
        }
    }
}

#endif