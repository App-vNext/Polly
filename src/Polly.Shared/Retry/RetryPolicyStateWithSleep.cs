using System;
using System.Collections.Generic;
using Polly.Utilities;

namespace Polly.Retry
{
    internal class RetryPolicyStateWithSleep : IRetryPolicyState
    {
        private readonly Action<Exception, TimeSpan, Context> _onRetry;
        private readonly Context _context;
        private readonly IEnumerator<TimeSpan> _sleepDurationsEnumerator;

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, Context> onRetry, Context context)
        {
            _onRetry = onRetry;
            _context = context;
            _sleepDurationsEnumerator = sleepDurations.GetEnumerator();
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan> onRetry) :
            this(sleepDurations, (exception, span, context) => onRetry(exception, span), null)
        {
        }

        public bool CanRetry(Exception ex)
        {
            if (!_sleepDurationsEnumerator.MoveNext()) return false;

            var currentTimeSpan = _sleepDurationsEnumerator.Current;
            _onRetry(ex, currentTimeSpan, _context);
                
            SystemClock.Sleep(currentTimeSpan);

            return true;
        }
    }
}