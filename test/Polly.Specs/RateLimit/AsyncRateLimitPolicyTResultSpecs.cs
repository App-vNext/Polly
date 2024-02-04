namespace Polly.Specs.RateLimit;

[Collection(Constants.SystemClockDependentTestCollection)]
public class AsyncRateLimitPolicyTResultSpecs : RateLimitPolicyTResultSpecsBase, IDisposable
{
    public void Dispose() =>
        SystemClock.Reset();

    protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan) =>
        Policy.RateLimitAsync<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan);

    protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst) =>
        Policy.RateLimitAsync<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan, maxBurst);

    protected override IRateLimitPolicy<TResult> GetPolicyViaSyntax<TResult>(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst,
        Func<TimeSpan, Context, TResult> retryAfterFactory) =>
        Policy.RateLimitAsync<TResult>(numberOfExecutions, perTimeSpan, maxBurst, retryAfterFactory);

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
            return typedPolicy.ExecuteAsync(_ => Task.FromResult(resultIfExecutionPermitted), context).GetAwaiter().GetResult();
        }
        else
        {
            throw new InvalidOperationException("Unexpected policy type in test construction.");
        }
    }
}
