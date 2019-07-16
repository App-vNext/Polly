﻿using System;
using System.Threading.Tasks;
using Polly.RateLimit;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.RateLimit;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.RateLimit
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class AsyncRateLimitPolicySpecs : RateLimitPolicySpecsBase, IDisposable
    {
        public void Dispose()
        {
            SystemClock.Reset();
        }

        protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan)
        {
            return Policy.RateLimitAsync(numberOfExecutions, perTimeSpan);
        }

        protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst)
        {
            return Policy.RateLimitAsync(numberOfExecutions, perTimeSpan, maxBurst);
        }
        
        protected override (bool, TimeSpan) TryExecuteThroughPolicy(IRateLimitPolicy policy)
        {
            if (policy is AsyncRateLimitPolicy typedPolicy)
            {
                try
                {
                    typedPolicy.ExecuteAsync(() => Task.FromResult(new ResultClassWithRetryAfter(ResultPrimitive.Good))).GetAwaiter().GetResult();
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
