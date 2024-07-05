#nullable enable

namespace Polly.Caching;

/// <summary>
/// Defines how a <see cref="CachePolicy"/> should get a string cache key from an execution <see cref="Context"/>.
/// </summary>
public interface ICacheKeyStrategy
{
    /// <summary>
    /// Gets the cache key from the given execution context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The cache key.</returns>
    string GetCacheKey(Context context);
}
