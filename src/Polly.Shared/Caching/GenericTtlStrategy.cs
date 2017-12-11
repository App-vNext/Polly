using System;
using System.Collections.Generic;
using System.Text;
using Polly.Utilities;

namespace Polly.Caching
{
    /// <summary>
    /// Represents a <see cref="ITtlStrategy"/> expiring based on the execution result.
    /// </summary>
    internal class GenericTtlStrategy<TResult> : ITtlStrategy<TResult>
    {
        private readonly ITtlStrategy _wrappedTtlStrategy;

        internal GenericTtlStrategy(ITtlStrategy ttlStrategy)
        {
            if (ttlStrategy == null) throw new ArgumentNullException(nameof(ttlStrategy));

            _wrappedTtlStrategy = ttlStrategy;
        }

        /// <summary>
        /// Gets a TTL for a cacheable item, given the current execution context and result.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="result">The execution result.</param>
        /// <returns>A <see cref="Ttl"/> representing the remaining Ttl of the cached item.</returns>
        public Ttl GetTtl(Context context, TResult result)
        {
            return _wrappedTtlStrategy.GetTtl(context, (object)result);
        }
    }
}
