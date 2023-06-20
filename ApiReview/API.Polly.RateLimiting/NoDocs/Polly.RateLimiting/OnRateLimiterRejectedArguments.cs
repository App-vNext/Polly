// Assembly 'Polly.RateLimiting'

using System;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;

namespace Polly.RateLimiting;

public record OnRateLimiterRejectedArguments(ResilienceContext Context, RateLimitLease Lease, TimeSpan? RetryAfter)
{
    [CompilerGenerated]
    protected virtual Type EqualityContract { get; }
}
