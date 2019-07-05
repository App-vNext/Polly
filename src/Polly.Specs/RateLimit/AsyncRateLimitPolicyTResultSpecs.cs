using System;
using System.Threading.Tasks;
using Polly.RateLimit;
using Polly.Specs.Helpers;
using Polly.Specs.Helpers.RateLimit;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.RateLimit
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class AsyncRateLimitPolicyTResultSpecs : RateLimitPolicyTResultSpecsBase, IDisposable
    {
        public void Dispose()
        {
            SystemClock.Reset();
        }

        protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan)
        {
            return Policy.RateLimitAsync<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan);
        }

        protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst)
        {
            return Policy.RateLimitAsync<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan, maxBurst);
        }

        protected override IRateLimitPolicy<TResult> GetPolicyViaSyntax<TResult>(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst,
            Func<TimeSpan, Context, TResult> retryAfterFactory)
        {
            return Policy.RateLimitAsync<TResult>(numberOfExecutions, perTimeSpan, maxBurst, retryAfterFactory);
        }

        protected override (bool, TimeSpan) TryExecuteThroughPolicy(IRateLimitPolicy policy)
        {
            if (policy is AsyncRateLimitPolicy<ResultClassWithRetryAfter> typedPolicy)
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

        protected override TResult TryExecuteThroughPolicy<TResult>(IRateLimitPolicy<TResult> policy, Context context, TResult resultIfExecutionPermitted)
        {
            if (policy is AsyncRateLimitPolicy<TResult> typedPolicy)
            {
                return typedPolicy.ExecuteAsync(ctx => Task.FromResult(resultIfExecutionPermitted), context).GetAwaiter().GetResult();
            }
            else
            {
                throw new InvalidOperationException("Unexpected policy type in test construction.");
            }
        }
    }
}
