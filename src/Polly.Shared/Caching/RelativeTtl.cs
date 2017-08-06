using System;
using Polly.Utilities;

namespace Polly.Caching
{
    /// <summary>
    /// Defines a ttl strategy which will cache items until the specified point-in-time.
    /// </summary>
    public class RelativeTtl : NonSlidingTtl
    {
        private static readonly TimeSpan DateTimeOffSetMaxTimeSpan = DateTimeOffset.MaxValue.Subtract(DateTimeOffset.MinValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeTtl"/> class.
        /// </summary>
        /// <param name="ttl">The timespan for which to consider the cache item valid.</param>
        public RelativeTtl(TimeSpan ttl) : base(
            ttl < TimeSpan.Zero ? throw new ArgumentOutOfRangeException(nameof(ttl), "The ttl for items to cache must be greater than zero.")
            : 
            ttl == TimeSpan.MaxValue ? DateTimeOffset.MaxValue
            : 
            ttl >= DateTimeOffSetMaxTimeSpan ? DateTimeOffset.MaxValue 
            : 
            SystemClock.DateTimeOffsetUtcNow() > DateTimeOffset.MaxValue.Subtract(ttl) ? DateTimeOffset.MaxValue  
            :
            SystemClock.DateTimeOffsetUtcNow().Add(ttl))
        {
        }
    }
}
