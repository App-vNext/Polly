using System;
using System.Collections.Generic;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleep : IRetryPolicyState
    {
        private int _errorCount;
        private readonly Action<Exception, TimeSpan, int, Context> _onRetry;
        private readonly Context _context;
        private readonly IEnumerator<TimeSpan> _sleepDurationsEnumerator;

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, int, Context> onRetry, Context context)
        {
            _onRetry = onRetry;
            _context = context;
            _sleepDurationsEnumerator = sleepDurations.GetEnumerator();
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, Context> onRetry, Context context) :
            this(sleepDurations, (exception, span, i, c) => onRetry(exception, span, context), Context.Empty)
        {
        }

        public RetryPolicyStateWithSleep(IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan> onRetry) :
            this(sleepDurations, (exception, span, context) => onRetry(exception, span), Context.Empty)
        {
        }

        public bool CanRetry(Exception ex)
        {
            if (!_sleepDurationsEnumerator.MoveNext()) return false;

            _errorCount += 1;

            var currentTimeSpan = _sleepDurationsEnumerator.Current;
            _onRetry(ex, currentTimeSpan, _errorCount, _context);
                
            SystemClock.Sleep(currentTimeSpan);

            return true;
        }
    }
}