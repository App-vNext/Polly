using System;

namespace Polly.Caching
{
    /// <summary>
    /// Defines a ttl strategy which will cache items for the specified TimeSpan.
    /// </summary>
    public class TimeSpanTtl : ITtlStrategy
    {
        private readonly TimeSpan ttl;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanTtl" /> class.
        /// </summary>
        /// <param name="ttl">Duration (ttl) for which to cache values.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public TimeSpanTtl(TimeSpan ttl)
        {
            if (ttl < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(ttl), "The ttl for items to cache must be greater than zero.");

            this.ttl = ttl;
        }

        /// <summary>
        /// Gets the TimeSpan for which to keep items cached.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>TimeSpan.</returns>
        public TimeSpan GetTtl(Context context) => ttl;
    }
}
