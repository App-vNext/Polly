using System;
using Polly.Utilities;

namespace Polly.Caching
{
    /// <summary>
    /// Defines a ttl strategy which will cache items until the specified point-in-time.
    /// </summary>
    public class PointInTimeTtl : ITtlStrategy
    {
        private readonly DateTimeOffset pointInTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointInTimeTtl"/> class.
        /// </summary>
        /// <param name="pointInTime">The point in time until which to keep items cached.</param>
        public PointInTimeTtl(DateTimeOffset pointInTime)
        {
            this.pointInTime = pointInTime;
        }

        /// <summary>
        /// Gets the TimeSpan to keep items cached using this Ttl strategy, such that they are kept until the specified point-in-time.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>TimeSpan.</returns>
        public TimeSpan GetTtl(Context context)
        {
            TimeSpan untilPointInTime = pointInTime.Subtract(SystemClock.DateTimeOffsetUtcNow());
            return untilPointInTime > TimeSpan.Zero ? untilPointInTime : TimeSpan.Zero;
        }
    }
}
