using System.Threading.RateLimiting;

namespace Polly.RateLimiting;

/// <summary>
/// The arguments used by the <see cref="RateLimiterStrategyOptions.OnRejected"/>.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Lease">The lease that has no permits and was rejected by the rate limiter.</param>
/// <param name="RetryAfter">The amount of time to wait before retrying again. This value is retrieved from the <see cref="Lease"/> by reading the <see cref="MetadataName.RetryAfter"/>.</param>
/// <remarks>
/// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
/// </remarks>
public readonly record struct OnRateLimiterRejectedArguments(ResilienceContext Context, RateLimitLease Lease, TimeSpan? RetryAfter);
