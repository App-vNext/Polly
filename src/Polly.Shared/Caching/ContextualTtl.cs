using System;

namespace Polly.Caching
{
    /// <summary>
    /// Defines a ttl strategy which will cache items for a TimeSpan which may be influenced by data in the execution context.
    /// </summary>
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

        private static readonly Ttl _noTtl = new Ttl(TimeSpan.Zero, false);

        /// <summary>
        /// Gets the TimeSpan for which to keep an item about to be cached, which may be influenced by data in the execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>TimeSpan.</returns>
        public Ttl GetTtl(Context context)
        {
            if (!context.ContainsKey(TimeSpanKey)) return _noTtl;
            bool sliding = context.ContainsKey(SlidingExpirationKey) ? context[SlidingExpirationKey] as bool? ?? false : false;
            return new Ttl(context[TimeSpanKey] as TimeSpan? ?? TimeSpan.Zero, sliding);
        }

    }
}
