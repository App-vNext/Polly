#if SUPPORTS_ASYNC

using System;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial class RetryPolicyState : IRetryPolicyState
    {
        static readonly Task<bool> Done = Task.FromResult(true);

        public Task<bool> CanRetryAsync(Exception ex)
        {
            _onRetry(ex, _context);
            return Done;
        }
    }
}

#endif