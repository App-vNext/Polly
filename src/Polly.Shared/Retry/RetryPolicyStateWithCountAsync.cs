#if SUPPORTS_ASYNC

using System;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial class RetryPolicyStateWithCount : IRetryPolicyState
    {
        public Task<bool> CanRetryAsync(Exception ex)
        {
            return Task.FromResult(CanRetry(ex));
        }
    }
}

#endif