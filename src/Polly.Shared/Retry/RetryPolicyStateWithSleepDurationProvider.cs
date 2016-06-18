using System;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithSleepDurationProvider<TResult> : IRetryPolicyState<TResult>
    {
        private int _errorCount;
        private readonly Func<int, TimeSpan> _sleepDurationProvider;
        private readonly Action<DelegateResult<TResult>, TimeSpan, Context> _onRetry;
        private readonly Context _context;

        public RetryPolicyStateWithSleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry, Context context)
        {
            this._sleepDurationProvider = sleepDurationProvider;
            _onRetry = onRetry;
            _context = context;
        }

        public RetryPolicyStateWithSleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan> onRetry) :
            this(sleepDurationProvider, (delegateResult, timespan, context) => onRetry(delegateResult, timespan), Context.Empty)
        {
        }

        public bool CanRetry(DelegateResult<TResult> delegateResult)
        {
            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }           

            var currentTimeSpan = _sleepDurationProvider(_errorCount);
            _onRetry(delegateResult, currentTimeSpan, _context);

            SystemClock.Sleep(currentTimeSpan);
            
            return true;
        }        
    }
}
