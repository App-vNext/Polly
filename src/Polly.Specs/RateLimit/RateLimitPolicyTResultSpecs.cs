using System;
using Polly.RateLimit;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.RateLimit;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.RateLimit
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class RateLimitPolicyTResultSpecs : RateLimitPolicyTResultSpecsBase, IDisposable
    {
        public void Dispose()
        {
            SystemClock.Reset();
        }

        protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan)
        {
            return Policy.RateLimit<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan);
        }

        protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst)
        {
            return Policy.RateLimit<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan, maxBurst);
        }

        protected override IRateLimitPolicy<TResult> GetPolicyViaSyntax<TResult>(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst,
            Func<TimeSpan, Context, TResult> retryAfterFactory)
        {
            return Policy.RateLimit<TResult>(numberOfExecutions, perTimeSpan, maxBurst, retryAfterFactory);
        }

        protected override (bool, TimeSpan) TryExecuteThroughPolicy(IRateLimitPolicy policy)
        {
            if (policy is RateLimitPolicy<ResultClassWithRetryAfter> typedPolicy)
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

        protected override TResult TryExecuteThroughPolicy<TResult>(IRateLimitPolicy<TResult> policy, Context context, TResult resultIfExecutionPermitted)
        {
            if (policy is RateLimitPolicy<TResult> typedPolicy)
            {
                return typedPolicy.Execute(ctx => resultIfExecutionPermitted, context);
            }
            else
            {
                throw new InvalidOperationException("Unexpected policy type in test construction.");
            }
        }
    }
}
