using System;

namespace Polly.Retry
{
    internal partial interface IRetryPolicyState
    {
        bool CanRetry(Exception ex);
    }
}