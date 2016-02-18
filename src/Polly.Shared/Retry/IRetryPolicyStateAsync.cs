#if SUPPORTS_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial interface IRetryPolicyState
    {
        Task<bool> CanRetryAsync(Exception ex, CancellationToken ct, bool continueOnCapturedContext);
    }
}

#endif