using System;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleepDurationProvider : IRetryPolicyState
    {
        private int _errorCount;
        private readonly Func<int, TimeSpan> _sleepDurationProvider;
        private readonly Action<Exception, TimeSpan, Context> _onRetry;
        private readonly Context _context;

        public RetryPolicyStateWithSleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan, Context> onRetry, Context context)
        {
            this._sleepDurationProvider = sleepDurationProvider;
            _onRetry = onRetry;
            _context = context;
        }

        public RetryPolicyStateWithSleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider, Action<Exception, TimeSpan> onRetry) :
            this(sleepDurationProvider, (exception, timespan, context) => onRetry(exception, timespan), Context.Empty)
        {
        }

        public bool CanRetry(Exception ex)
        {
            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }           

            var currentTimeSpan = _sleepDurationProvider(_errorCount);
            _onRetry(ex, currentTimeSpan, _context);

            SystemClock.Sleep(currentTimeSpan);
            
            return true;
        }        
    }
}
