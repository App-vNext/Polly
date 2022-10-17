using System;
using Polly.RateLimit;

namespace Polly.Specs.RateLimit;

public abstract class SlidingWindowRateLimiterTestsBase : RateLimitSpecsBase
{
    protected abstract IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan);
}