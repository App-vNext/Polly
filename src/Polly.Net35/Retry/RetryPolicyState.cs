using System;

namespace Polly.Retry
{
    internal class RetryPolicyState : IRetryPolicyState
    {
        private readonly Action<Exception> _onRetry;

        public RetryPolicyState(Action<Exception> onRetry)
        {
            _onRetry = onRetry;
        }

        public bool CanRetry(Exception ex)
        {
            _onRetry(ex);
            return true;
        }
    }
}