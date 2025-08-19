using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;

namespace Polly.Caching;

/// <summary>
/// Represents options for the caching strategy.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Addressed with DynamicDependency on ValidationHelper.Validate method")]
public class CacheStrategyOptions<TResult> : ResilienceStrategyOptions
{
	/// <summary>
	/// Gets or sets the memory cache instance to use.
	/// </summary>
	[Required]
	public IMemoryCache? Cache { get; set; }

	/// <summary>
	/// Gets or sets the time-to-live for cached entries.
	/// </summary>
	[Range(typeof(TimeSpan), "00:00:00", "365.00:00:00")]
	public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(5);

	/// <summary>
	/// Gets or sets a value indicating whether sliding expiration should be used.
	/// </summary>
	public bool UseSlidingExpiration { get; set; }

	/// <summary>
	/// Gets or sets a function that generates the cache key from the resilience context.
	/// If null, <see cref="ResilienceContext.OperationKey"/> is used.
	/// </summary>
	public Func<ResilienceContext, string?>? CacheKeyGenerator { get; set; }
}
