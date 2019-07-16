using System;
using Polly.RateLimit;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.RateLimit;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.RateLimit
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class RateLimitPolicySpecs : RateLimitPolicySpecsBase, IDisposable
    {
        public void Dispose()
        {
            SystemClock.Reset();
        }

        protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan)
        {
            return Policy.RateLimit(numberOfExecutions, perTimeSpan);
        }

        protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst)
        {
            return Policy.RateLimit(numberOfExecutions, perTimeSpan, maxBurst);
        }

        protected override (bool, TimeSpan) TryExecuteThroughPolicy(IRateLimitPolicy policy)
        {
            if (policy is RateLimitPolicy typedPolicy)
            {
                try
                {
                    typedPolicy.Execute(() => new ResultClassWithRetryAfter(ResultPrimitive.Good));
                    return (true, TimeSpan.Zero);
                }
                catch (RateLimitRejectedException e)
                {
                    return (false, e.RetryAfter);
                }
            }
            else
            {
                throw new InvalidOperationException("Unexpected policy type in test construction.");
            }
        }
    }
}