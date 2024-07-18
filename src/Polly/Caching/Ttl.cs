#nullable enable
namespace Polly.Caching;

/// <summary>
/// Represents a time-to-live for a given cache item.
/// </summary>
#pragma warning disable CA1815 //Override equals and operator equals on value types
public struct Ttl
#pragma warning restore CA1815
{
    /// <summary>
    /// The timespan for which this cache-item remains valid.
    /// </summary>
    public TimeSpan Timespan;

    /// <summary>
    /// Whether this <see cref="Ttl"/> should be considered as sliding expiration: that is, the cache item should be considered valid for a further period of duration <see cref="Timespan"/> each time the cache item is retrieved.
    /// </summary>
    public bool SlidingExpiration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ttl"/> struct.
    /// </summary>
    /// <param name="timeSpan">The timespan for which this cache-item remains valid.
    /// <remarks>Will be considered as not denoting sliding expiration.</remarks></param>
    public Ttl(TimeSpan timeSpan)
        : this(timeSpan, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ttl"/> struct.
    /// </summary>
    /// <param name="timeSpan">The timespan for which this cache-item remains valid.</param>
    /// <param name="slidingExpiration">Whether this <see cref="Ttl"/> should be considered as sliding expiration.</param>
    public Ttl(TimeSpan timeSpan, bool slidingExpiration)
    {
        Timespan = timeSpan;
        SlidingExpiration = slidingExpiration;
    }
}
