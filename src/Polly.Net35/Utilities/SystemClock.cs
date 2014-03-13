using System;
using System.Threading;

#if PORTABLE
using System.Threading.Tasks;
#endif

namespace Polly.Utilities
{
    /// <summary>
    /// Time related delegates used to improve testability of the code
    /// </summary>
    public static class SystemClock
    {
        /// <summary>
        /// Allows the setting of a custom Thread.Sleep implementation for testing.
        /// By default this will be a call to <see cref="Thread.Sleep(TimeSpan)"/>
        /// </summary>
#if !PORTABLE
        public static Action<TimeSpan> Sleep = Thread.Sleep;
#endif
#if PORTABLE
        public static Action<TimeSpan> Sleep = async span => await Task.Delay(span);
#endif
        /// <summary>
        /// Allows the setting of a custom DateTime.UtcNow implementation for testing.
        /// By default this will be a call to <see cref="DateTime.UtcNow"/>
        /// </summary>
        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;

        /// <summary>
        /// Resets the custom implementations to their defaults. 
        /// Should be called during test teardowns.
        /// </summary>
        public static void Reset()
        {
#if !PORTABLE
        Sleep = Thread.Sleep;
#endif
#if PORTABLE
            Sleep = async span => await Task.Delay(span);
#endif
            UtcNow = () => DateTime.UtcNow;
        }
    }
}