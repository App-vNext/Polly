#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines a ttl strategy which will cache items for the specified time.
/// </summary>
public class RelativeTtl : ITtlStrategy
{
    private readonly TimeSpan _ttl;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelativeTtl"/> class.
    /// </summary>
    /// <param name="ttl">The timespan for which to consider the cache item valid.</param>
    public RelativeTtl(TimeSpan ttl)
    {
        if (ttl < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), "The ttl for items to cache must be greater than zero.");
        }

        _ttl = ttl;
    }

    /// <summary>
    /// Gets a TTL for a cacheable item, given the current execution context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="result">The execution result.</param>
    /// <returns>A <see cref="Ttl"/> representing the remaining Ttl of the cached item.</returns>
    public Ttl GetTtl(Context context, object? result) => new(_ttl);
}
