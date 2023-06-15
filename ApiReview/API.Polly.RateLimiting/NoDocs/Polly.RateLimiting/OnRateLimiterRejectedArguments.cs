// Assembly 'Polly.RateLimiting'

using System;
using System.Threading.RateLimiting;

namespace Polly.RateLimiting;

public readonly record struct OnRateLimiterRejectedArguments(ResilienceContext Context, RateLimitLease Lease, TimeSpan? RetryAfter);
