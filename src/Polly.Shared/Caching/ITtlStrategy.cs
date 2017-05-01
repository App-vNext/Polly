﻿using System;

namespace Polly.Caching
{
    /// <summary>
    /// Defines a strategy for providing time-to-live durations for cacheable results.
    /// </summary>
    public interface ITtlStrategy
    {
        /// <summary>
        /// Gets a TTL for a cacheable item, given the current execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>TimeSpan</returns>
        TimeSpan GetTtl(Context context);
    }
}
