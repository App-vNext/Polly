using System;

namespace Polly.Retry
{
    internal partial interface IRetryPolicyState<TResult>
    {
        bool CanRetry(DelegateResult<TResult> delegateResult);
    }
}