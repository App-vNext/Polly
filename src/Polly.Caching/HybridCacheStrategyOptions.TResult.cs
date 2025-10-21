using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Hybrid;

namespace Polly.Caching;

/// <summary>
/// Options for the HybridCache-based caching strategy for typed resilience pipelines.
/// </summary>
/// <typeparam name="TResult">The result type of the resilience pipeline.</typeparam>
/// <remarks>
/// This strategy is designed for use with typed resilience pipelines (<see cref="ResiliencePipelineBuilder{TResult}"/>).
/// HybridCache requires concrete types for proper serialization and deserialization support.
/// </remarks>
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Members preserved via builder validation.")]
public class HybridCacheStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets or sets the <see cref="HybridCache"/> instance to use.
    /// </summary>
    [Required]
    public HybridCache? Cache { get; set; }

    /// <summary>
    /// Gets or sets the time-to-live for cached entries.
    /// The default is 5 minutes.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00", "365.00:00:00")]
    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets a value indicating whether sliding expiration should be used.
    /// The default is <see langword="false"/>.
    /// </summary>
    public bool UseSlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets a delegate that generates the cache key from the resilience context.
    /// If <see langword="null"/>, <see cref="ResilienceContext.OperationKey"/> is used.
    /// </summary>
    public Func<ResilienceContext, string?>? CacheKeyGenerator { get; set; }
}
