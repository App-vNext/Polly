using System;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithCount : IRetryPolicyState
    {
        private int _errorCount;
        private readonly int _retryCount;
        private readonly Action<Exception, int, Context> _onRetry;
        private readonly Context _context;

        public RetryPolicyStateWithCount(int retryCount, Action<Exception, int, Context> onRetry, Context context)
        {
            _retryCount = retryCount;
            _onRetry = onRetry;
            _context = context;
        }

        public RetryPolicyStateWithCount(int retryCount, Action<Exception, int> onRetry) :
            this(retryCount, (exception, i, context) => onRetry(exception, i), Context.Empty)
        {
        }

        public bool CanRetry(Exception ex)
        {
            _errorCount += 1;

            var shouldRetry = _errorCount <= _retryCount;
            if (shouldRetry)
            {
                _onRetry(ex, _errorCount, _context);
            }

            return shouldRetry;
        }
    }
}