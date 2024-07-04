#nullable enable

namespace Polly.Caching;

#pragma warning disable CA1062 // Validate arguments of public methods // Temporary stub

/// <summary>
/// The default cache key strategy for <see cref="CachePolicy"/>.  Returns the property <see cref="Context.OperationKey"/>.
/// </summary>
public class DefaultCacheKeyStrategy : ICacheKeyStrategy
{
    /// <summary>
    /// Gets the cache key from the given execution context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The cache key.</returns>
    public string GetCacheKey(Context context) =>
        context.OperationKey;

    /// <summary>
    /// Gets an instance of the <see cref="DefaultCacheKeyStrategy"/>.
    /// </summary>
    public static readonly ICacheKeyStrategy Instance = new DefaultCacheKeyStrategy();
}
