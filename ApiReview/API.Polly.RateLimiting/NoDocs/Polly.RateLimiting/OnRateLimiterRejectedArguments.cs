// Assembly 'Polly.RateLimiting'

using System;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;

namespace Polly.RateLimiting;

public readonly struct OnRateLimiterRejectedArguments
{
    public ResilienceContext Context { get; }
    public RateLimitLease Lease { get; }
    public TimeSpan? RetryAfter { get; }
    public OnRateLimiterRejectedArguments(ResilienceContext context, RateLimitLease lease, TimeSpan? retryAfter);
}
