#nullable enable

using System;

namespace Polly.Caching;

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
    public string GetCacheKey(Context context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return context.OperationKey;
    }

    /// <summary>
    /// Gets an instance of the <see cref="DefaultCacheKeyStrategy"/>.
    /// </summary>
    public static readonly ICacheKeyStrategy Instance = new DefaultCacheKeyStrategy();
}
