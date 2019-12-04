﻿using System;

namespace Polly.Caching
{
    /// <summary>
    /// Defines a ttl strategy which will cache items with a sliding ttl.
    /// </summary>

    public class SlidingTtl : ITtlStrategy
    {
        private readonly Ttl ttl;

        /// <summary>
        /// Constructs a new instance of the <see cref="SlidingTtl"/> ttl strategy.
        /// </summary>
        /// <param name="slidingTtl">The sliding timespan for which cache items should be considered valid.</param>
        public SlidingTtl(TimeSpan slidingTtl)
        {
            if (slidingTtl < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(slidingTtl), "The ttl for items to cache must be greater than zero.");

            ttl = new Ttl(slidingTtl, true);
        }

        /// <summary>
        /// Gets a TTL for the cacheable item.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="result">The execution result.</param>
        /// <returns>A <see cref="Ttl"/> representing the remaining Ttl of the cached item.</returns>

        public Ttl GetTtl(Context context, object result) => ttl;
    }
}
