// Assembly 'Polly.RateLimiting'

using System;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Polly.RateLimiting;

public sealed class ResilienceRateLimiter : IDisposable, IAsyncDisposable
{
    public static ResilienceRateLimiter Create(RateLimiter rateLimiter);
    public static ResilienceRateLimiter Create(PartitionedRateLimiter<ResilienceContext> rateLimiter);
    public ValueTask DisposeAsync();
    public void Dispose();
}
