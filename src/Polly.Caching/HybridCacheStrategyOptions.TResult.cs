using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Hybrid;

namespace Polly.Caching;

/// <summary>
/// Options for the HybridCache-based caching strategy.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Members preserved via builder validation.")]
public class HybridCacheStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>Gets or sets the HybridCache instance to use.</summary>
    [Required]
    public HybridCache? Cache { get; set; }

    /// <summary>Gets or sets the time-to-live for cached entries.</summary>
    [Range(typeof(TimeSpan), "00:00:00", "365.00:00:00")]
    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Gets or sets a value indicating whether sliding expiration should be used.</summary>
    public bool UseSlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets a function that generates the cache key from the resilience context.
    /// If <see langword="null"/>, <see cref="ResilienceContext.OperationKey"/> is used.
    /// </summary>
    public Func<ResilienceContext, string?>? CacheKeyGenerator { get; set; }
}
