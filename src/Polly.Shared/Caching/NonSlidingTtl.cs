using System;
using System.Collections.Generic;
using System.Text;
using Polly.Utilities;

namespace Polly.Caching
{
    /// <summary>
    /// Represents an <see cref="ITtlStrategy"/> expiring at an absolute time, not with sliding expiration.
    /// </summary>
    public abstract class NonSlidingTtl : ITtlStrategy
    {
        /// <summary>
        /// The absolute expiration time for cache items, represented by this strategy.
        /// </summary>
        protected readonly DateTimeOffset absoluteExpirationTime;

        /// <summary>
        /// Constructs a new instance of the <see cref="NonSlidingTtl"/> strategy.
        /// </summary>
        /// <param name="absoluteExpirationTime">The absolute expiration time for cache items, represented by this strategy.</param>
        protected NonSlidingTtl(DateTimeOffset absoluteExpirationTime)
        {
            this.absoluteExpirationTime = absoluteExpirationTime;
        }

        /// <summary>
        /// Gets a TTL for a cacheable item, given the current execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>A <see cref="Ttl"/> representing the remaining Ttl of the cached item.</returns>
        public Ttl GetTtl(Context context)
        {
            TimeSpan untilPointInTime = absoluteExpirationTime.Subtract(SystemClock.DateTimeOffsetUtcNow());
            TimeSpan remaining = untilPointInTime > TimeSpan.Zero ? untilPointInTime : TimeSpan.Zero;
            return new Ttl(remaining, false);
        }
    }
}
