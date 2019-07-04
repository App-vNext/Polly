using System;
using Polly.RateLimit;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.RateLimit
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class AsyncRateLimitPolicyTResultSpecs : RateLimitPolicySpecsBase, IDisposable
    {
        public void Dispose()
        {
            SystemClock.Reset();
        }

        public override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan)
        {
            return Policy.RateLimitAsync<ResultClass>(numberOfExecutions, perTimeSpan);
        }

        public override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst)
        {
            return Policy.RateLimitAsync<ResultClass>(numberOfExecutions, perTimeSpan, maxBurst);
        }
    }
}
