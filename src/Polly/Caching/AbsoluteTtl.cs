#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines a ttl strategy which will cache items until the specified point-in-time.
/// </summary>
public class AbsoluteTtl : NonSlidingTtl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbsoluteTtl"/> class.
    /// </summary>
    /// <param name="absoluteExpirationTime">The UTC point in time until which to consider the cache item valid.</param>
    public AbsoluteTtl(DateTimeOffset absoluteExpirationTime)
        : base(absoluteExpirationTime)
    {
    }
}
