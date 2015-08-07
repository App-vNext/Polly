using System;

namespace Polly.Retry
{
    internal class RetryPolicyStateWithIteration : IRetryPolicyState
    {
        private int _errorCount;
        private readonly Action<Exception, int, Context> _onRetry;
        private readonly Context _context;

        public RetryPolicyStateWithIteration(Action<Exception, int, Context> onRetry, Context context)
        {
            _onRetry = onRetry;
            _context = context;
        }

        public RetryPolicyStateWithIteration(Action<Exception, int> onRetry) :
            this((exception, i, context) => onRetry(exception, i), null)
        {
        }

        public bool CanRetry(Exception ex)
        {
            _errorCount += 1;
            _onRetry(ex, _errorCount, _context);

            return true;
        }
    }
}