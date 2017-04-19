using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryStateWaitAndRetryForever<TResult> : IRetryPolicyState<TResult>
    {
        private int _errorCount;
        private readonly Func<int, Context, TimeSpan> _sleepDurationProvider;
        private readonly Action<DelegateResult<TResult>, TimeSpan, Context> _onRetry;
        private readonly Context _context;

        public RetryStateWaitAndRetryForever(Func<int, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry, Context context)
        {
            _sleepDurationProvider = sleepDurationProvider;
            _onRetry = onRetry;
            _context = context;
        }

        public bool CanRetry(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken)
        {
            if (_errorCount < int.MaxValue)
            {
                _errorCount += 1;
            }           

            var currentTimeSpan = _sleepDurationProvider(_errorCount, _context);
            _onRetry(delegateResult, currentTimeSpan, _context);

            SystemClock.Sleep(currentTimeSpan, cancellationToken);
            
            return true;
        }        
    }
}
