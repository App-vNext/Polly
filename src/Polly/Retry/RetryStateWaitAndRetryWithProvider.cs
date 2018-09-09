using System;
using System.Threading;
using Polly.Utilities;

namespace Polly.Retry
{
    internal partial class RetryStateWaitAndRetryWithProvider<TResult> : IRetryPolicyState<TResult>
    {
        private int _errorCount;
        private readonly int _retryCount;
        private readonly Func<int, DelegateResult<TResult>, Context, TimeSpan> _sleepDurationProvider;
        private readonly Action<DelegateResult<TResult>, TimeSpan, int, Context> _onRetry;
        private readonly Context _context;

        public RetryStateWaitAndRetryWithProvider(int retryCount, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry, Context context)
        {
            _retryCount = retryCount;
            _sleepDurationProvider = sleepDurationProvider;
            _onRetry = onRetry;
            _context = context;
        }
        
        public bool CanRetry(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken)
        {
            _errorCount += 1;

            bool shouldRetry = _errorCount <= _retryCount;
            if (shouldRetry)
            {
                TimeSpan waitTimeSpan = _sleepDurationProvider(_errorCount, delegateResult, _context);

                _onRetry(delegateResult, waitTimeSpan, _errorCount, _context);

                SystemClock.Sleep(waitTimeSpan, cancellationToken);
            }

            return shouldRetry;
        }
        
    }
}
