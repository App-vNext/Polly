using System;

namespace Polly.Duration
{
    /// <summary>
    /// 
    /// </summary>
    public static class Jitter
    {
        [ThreadStatic]
        private static readonly Random s_random = new Random(); // Default ctor uses a time-based seed

        /// <summary>
        /// A shared, thread-safe (thread-static) instance of <see cref="Random"/>.
        /// </summary>
        public static Random Random => s_random;
    }
}
