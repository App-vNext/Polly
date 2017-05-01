using System;

namespace Polly.Caching
{
    /// <summary>
    /// Defines a ttl strategy which will cache items for a TimeSpan which may be influenced by data in the execution context.
    /// </summary>
    public class ContextualTimeSpanTtl : ITtlStrategy
    {
        /// <summary>
        /// The key on the execution <see cref="Context"/> to use for storing the Ttl TimeSpan for which to cache.
        /// </summary>
        public static readonly string Key = "ContextualTimeSpanTtl";

        /// <summary>
        /// Gets the TimeSpan for which to keep an item about to be cached, which may be influenced by data in the execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>TimeSpan.</returns>
        public TimeSpan GetTtl(Context context)
        {
            if (!context.ContainsKey(Key)) return TimeSpan.Zero;
            return context[Key] as TimeSpan? ?? TimeSpan.Zero;
        }

    }
}
