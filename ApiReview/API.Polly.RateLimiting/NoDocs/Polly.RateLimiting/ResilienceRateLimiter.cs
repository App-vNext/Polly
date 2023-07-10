// Assembly 'Polly.RateLimiting'

using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Polly.RateLimiting;

public sealed class ResilienceRateLimiter
{
    public static ResilienceRateLimiter Create(RateLimiter rateLimiter);
    public static ResilienceRateLimiter Create(PartitionedRateLimiter<ResilienceContext> rateLimiter);
}
