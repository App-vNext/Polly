using System;

namespace Polly.Retry
{
    internal class RetryPolicyStateWithCount : IRetryPolicyState
    {
        private int _errorCount;
        private readonly int _retryCount;
        private readonly Action<Exception, int> _onRetry;

        public RetryPolicyStateWithCount(int retryCount, Action<Exception, int> onRetry)
        {
            _retryCount = retryCount;
            _onRetry = onRetry;
        }

        public bool CanRetry(Exception ex)
        {
            _errorCount += 1;

            var shouldRetry = _errorCount <= _retryCount;
            if (shouldRetry)
            {
                _onRetry(ex, _errorCount);
            }

            return shouldRetry;
        }
    }
}