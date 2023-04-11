using System.Threading.RateLimiting;
using Polly.Strategy;

namespace Polly.RateLimiting;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// The arguments used by the <see cref="OnRateLimiterRejectedEvent"/>.
/// </summary>
public readonly struct OnRateLimiterRejectedArguments : IResilienceArguments
{
    internal OnRateLimiterRejectedArguments(ResilienceContext context, RateLimitLease lease, TimeSpan? retryAfter)
    {
        Context = context;
        Lease = lease;
        RetryAfter = retryAfter;
    }

    /// <inheritdoc/>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the lease that has no permits and was rejected by the rate limiter.
    /// </summary>
    public RateLimitLease Lease { get; }

    /// <summary>
    /// Gets the amount of time to wait before retrying again.
    /// </summary>
    /// <remarks>
    /// This value is retrieved from the <see cref="Lease"/> by reading the <see cref="MetadataName.RetryAfter"/>.
    /// </remarks>
    public TimeSpan? RetryAfter { get; }
}
