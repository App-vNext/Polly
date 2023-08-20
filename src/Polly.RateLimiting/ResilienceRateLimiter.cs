using System.Threading.RateLimiting;

namespace Polly.RateLimiting;

/// <summary>
/// This class is just a simple adapter for the built-in limiters in the <c>System.Threading.RateLimiting</c> namespace.
/// </summary>
public sealed class ResilienceRateLimiter : IDisposable, IAsyncDisposable
{
    private ResilienceRateLimiter(RateLimiter? limiter, PartitionedRateLimiter<ResilienceContext>? partitionedLimiter)
    {
        Limiter = limiter;
        PartitionedLimiter = partitionedLimiter;
    }

    /// <summary>
    /// Creates an instance of <see cref="ResilienceRateLimiter"/> from <paramref name="rateLimiter"/>.
    /// </summary>
    /// <param name="rateLimiter">The rate limiter instance.</param>
    /// <returns>An instance of <see cref="ResilienceRateLimiter"/>.</returns>
    public static ResilienceRateLimiter Create(RateLimiter rateLimiter) => new(Guard.NotNull(rateLimiter), null);

    /// <summary>
    /// Creates an instance of <see cref="ResilienceRateLimiter"/> from partitioned <paramref name="rateLimiter"/>.
    /// </summary>
    /// <param name="rateLimiter">The rate limiter instance.</param>
    /// <returns>An instance of <see cref="ResilienceRateLimiter"/>.</returns>
    public static ResilienceRateLimiter Create(PartitionedRateLimiter<ResilienceContext> rateLimiter) => new(null, Guard.NotNull(rateLimiter));

    internal RateLimiter? Limiter { get; }

    internal PartitionedRateLimiter<ResilienceContext>? PartitionedLimiter { get; }

    internal ValueTask<RateLimitLease> AcquireAsync(ResilienceContext context)
    {
        if (PartitionedLimiter is not null)
        {
            return PartitionedLimiter.AcquireAsync(context, permitCount: 1, context.CancellationToken);
        }
        else
        {
            return Limiter!.AcquireAsync(permitCount: 1, context.CancellationToken);
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (PartitionedLimiter is not null)
        {
            return PartitionedLimiter.DisposeAsync();
        }
        else
        {
            return Limiter!.DisposeAsync();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (PartitionedLimiter is not null)
        {
            PartitionedLimiter.Dispose();
        }
        else
        {
            Limiter!.Dispose();
        }
    }
}
