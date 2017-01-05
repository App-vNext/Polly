

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Retry
{
    internal partial interface IRetryPolicyState<TResult>
    {
        Task<bool> CanRetryAsync(DelegateResult<TResult> delegateResult, CancellationToken ct, bool continueOnCapturedContext);
    }
}

