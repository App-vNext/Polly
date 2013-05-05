using System;
using System.Collections.Generic;
using Polly.Utilities;

namespace Polly.Retry
{
    internal class RetryPolicyStateWithSleep : IRetryPolicyState
    {
        private readonly Action<Exception, TimeSpan> _onRetry;
        private readonly IEnumerator<TimeSpan> _sleepDurationsEnumerator;

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan> onRetry)
        {
            _sleepDurationsEnumerator = sleepDurations.GetEnumerator();
            _onRetry = onRetry;
        }

        public bool CanRetry(Exception ex)
        {
            if (!_sleepDurationsEnumerator.MoveNext()) return false;

            var currentTimeSpan = _sleepDurationsEnumerator.Current;
            _onRetry(ex, currentTimeSpan);
                
            SystemClock.Sleep(currentTimeSpan);

            return true;
        }
    }
}