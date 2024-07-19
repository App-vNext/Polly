#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines a ttl strategy which will cache items for a TimeSpan which may be influenced by data in the execution context.
/// </summary>
#pragma warning disable CA1062 // Validate arguments of public methods
public class ContextualTtl : ITtlStrategy
{
    /// <summary>
    /// The key on the execution <see cref="Context"/> to use for storing the Ttl TimeSpan for which to cache.
    /// </summary>
    public static readonly string TimeSpanKey = "ContextualTtlTimeSpan";

    /// <summary>
    /// The key on the execution <see cref="Context"/> to use for storing whether the Ttl should be treated as sliding expiration.
    /// <remarks>If no value is provided for this key, a ttl will not be treated as sliding expiration.</remarks>
    /// </summary>
    public static readonly string SlidingExpirationKey = "ContextualTtlSliding";

    private static readonly Ttl NoTtl = new(TimeSpan.Zero, false);

    /// <summary>
    /// Gets the TimeSpan for which to keep an item about to be cached, which may be influenced by data in the execution context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="result">The execution result.</param>
    /// <returns>TimeSpan.</returns>
    public Ttl GetTtl(Context context, object? result)
    {
        if (!context.ContainsKey(TimeSpanKey))
        {
            return NoTtl;
        }

        bool sliding = false;

        if (context.TryGetValue(SlidingExpirationKey, out object objValue))
        {
            sliding = objValue as bool? ?? false;
        }

        return new Ttl(context[TimeSpanKey] as TimeSpan? ?? TimeSpan.Zero, sliding);
    }
}
