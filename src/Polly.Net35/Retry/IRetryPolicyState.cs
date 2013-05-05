using System;

namespace Polly.Retry
{
    internal interface IRetryPolicyState
    {
        bool CanRetry(Exception ex);
    }
}