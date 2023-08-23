// Assembly 'Polly.RateLimiting'

using System;
using System.Runtime.CompilerServices;

namespace Polly.RateLimiting;

public sealed class RateLimiterRejectedException : ExecutionRejectedException
{
    public TimeSpan? RetryAfter { get; }
    public RateLimiterRejectedException();
    public RateLimiterRejectedException(TimeSpan retryAfter);
    public RateLimiterRejectedException(string message);
    public RateLimiterRejectedException(string message, TimeSpan retryAfter);
    public RateLimiterRejectedException(string message, Exception inner);
    public RateLimiterRejectedException(string message, TimeSpan retryAfter, Exception inner);
}
