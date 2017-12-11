using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Caching
{
    /// <summary>
    /// Defines a ttl strategy which will cache items with some calculation from the result of the execution.
    /// </summary>

    public class ResultTtl<TResult> : ITtlStrategy<TResult>
    {
        private readonly Func<TResult, Ttl> ttlFunc;

        /// <summary>
        /// Constructs a new instance of the <see cref="ResultTtl{TResult}"/> ttl strategy.
        /// </summary>
        /// <param name="ttlFunc">The function to calculate the TTL for which cache items should be considered valid.</param>
        public ResultTtl(Func<TResult, Ttl> ttlFunc)
        {
            if (ttlFunc == null) throw new ArgumentNullException(nameof(ttlFunc));

            this.ttlFunc = ttlFunc;
        }

        /// <summary>
        /// Gets a TTL for the cacheable item.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="result">The execution result.</param>
        /// <returns>A <see cref="Ttl"/> representing the remaining Ttl of the cached item.</returns>

        public Ttl GetTtl(Context context, TResult result)
        {
            return ttlFunc(result);
        }
    }
}
