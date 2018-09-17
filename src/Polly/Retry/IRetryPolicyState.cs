using System;
using System.Threading;

namespace Polly.Retry
{
    internal partial interface IRetryPolicyState<TResult>
    {
        bool CanRetry(DelegateResult<TResult> delegateResult, CancellationToken cancellationToken);
    }
}