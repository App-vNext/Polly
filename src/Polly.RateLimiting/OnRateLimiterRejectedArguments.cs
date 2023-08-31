using System.Threading.RateLimiting;

namespace Polly.RateLimiting;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// The arguments used by the <see cref="RateLimiterStrategyOptions.OnRejected"/>.
/// </summary>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly struct OnRateLimiterRejectedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnRateLimiterRejectedArguments"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    /// <param name="lease">The lease that has no permits and was rejected by the rate limiter.</param>
    public OnRateLimiterRejectedArguments(ResilienceContext context, RateLimitLease lease)
    {
        Context = context;
        Lease = lease;
    }

    /// <summary>
    /// Gets the context associated with the execution of a user-provided callback.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the lease that has no permits and was rejected by the rate limiter.
    /// </summary>
    public RateLimitLease Lease { get; }
}
