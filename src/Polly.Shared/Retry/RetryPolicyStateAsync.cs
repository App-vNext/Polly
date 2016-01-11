#if SUPPORTS_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial class RetryPolicyState : IRetryPolicyState
    {
        static readonly Task<bool> Done = Task.FromResult(true);

        public Task<bool> CanRetryAsync(Exception ex, CancellationToken ct, bool continueOnCapturedContext)
        {
            _onRetry(ex, _context);
            return Done;
        }
    }
}

#endif