using System;

namespace Polly.Retry
{
    internal partial class RetryPolicyState : IRetryPolicyState
    {
        private readonly Action<Exception, Context> _onRetry;
        private readonly Context _context;

        public RetryPolicyState(Action<Exception, Context> onRetry, Context context)
        {
            _onRetry = onRetry;
            _context = context;
        }

        public RetryPolicyState(Action<Exception> onRetry) :
            this((exception, context) => onRetry(exception), Context.Empty)
        {
        }

        public bool CanRetry(Exception ex)
        {
            _onRetry(ex, _context);
            return true;
        }
    }
}