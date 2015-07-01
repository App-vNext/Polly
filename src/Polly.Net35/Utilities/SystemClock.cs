using System;
using System.Threading;
#if PORTABLE || DNXCORE50
using System.Threading.Tasks;
#endif

namespace Polly.Utilities
{
    /// <summary>
    /// Time related delegates used to improve testability of the code
    /// </summary>
    public static class SystemClock
    {

#if !PORTABLE && !DNXCORE50
        /// <summary>
        /// Allows the setting of a custom Thread.Sleep implementation for testing.
        /// By default this will be a call to <see cref="Thread.Sleep(TimeSpan)"/>
        /// </summary>
        public static Action<TimeSpan> Sleep = Thread.Sleep;
#endif
#if PORTABLE || DNXCORE50
        /// <summary>
        /// Allows the setting of a custom Thread.Sleep implementation for testing.
        /// </summary>
        public static Action<TimeSpan> Sleep = timespan => new ManualResetEvent(false).WaitOne(timespan);
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


#if !PORTABLE && !DNXCORE50
        Sleep = Thread.Sleep;
#endif
#if PORTABLE || DNXCORE50
            Sleep = async span => await Task.Delay(span);
#endif
            UtcNow = () => DateTime.UtcNow;
        }
    }
}