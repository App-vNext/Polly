using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Utilities
{
    /// <summary>
    /// Time related delegates used to support different compilation targets and to improve testability of the code.
    /// </summary>
    public static class SystemClock
    {

#if NETSTANDARD2_0
        private static AsyncLocal<Action<TimeSpan, CancellationToken>> _sleep = new AsyncLocal<Action<TimeSpan, CancellationToken>>();
        private static AsyncLocal<Func<TimeSpan, CancellationToken, Task>> _sleepAsync = new AsyncLocal<Func<TimeSpan, CancellationToken, Task>>();
        private static AsyncLocal<Func<DateTime>> _utcNow = new AsyncLocal<Func<DateTime>>();
#endif

        /// <summary>
        /// Allows the setting of a custom Thread.Sleep implementation for testing.
        /// By default this will use the <see cref="CancellationToken"/>'s <see cref="WaitHandle"/>.
        /// </summary>
#if !NETSTANDARD2_0
        public static Action<TimeSpan, CancellationToken> Sleep = (timeSpan, cancellationToken) =>
        {
            if (cancellationToken.WaitHandle.WaitOne(timeSpan)) cancellationToken.ThrowIfCancellationRequested();
        };
#else
        public static Action<TimeSpan, CancellationToken> Sleep
        {
            get
            {
                if (_sleep.Value == null)
                    _sleep.Value = (timeSpan, cancellationToken) =>
                    {
                        if (cancellationToken.WaitHandle.WaitOne(timeSpan)) cancellationToken.ThrowIfCancellationRequested();
                    };
                return _sleep.Value;
            }
            set { _sleep.Value = value; }
        }
#endif

        /// <summary>
        /// Allows the setting of a custom async Sleep implementation for testing.
        /// By default this will be a call to <see cref="M:Task.Delay"/> taking a <see cref="CancellationToken"/>
        /// </summary>
#if NET40
        public static Func<TimeSpan, CancellationToken, Task> SleepAsync = TaskEx.Delay;
#elif NETSTANDARD2_0
        public static Func<TimeSpan, CancellationToken, Task> SleepAsync
        {
            get
            {
                if (_sleepAsync.Value == null)
                    _sleepAsync.Value = Task.Delay;
                return _sleepAsync.Value;
            }
            set { _sleepAsync.Value = value; }
        }
#else
        public static Func<TimeSpan, CancellationToken, Task> SleepAsync = Task.Delay;
#endif
        /// <summary>
        /// Allows the setting of a custom DateTime.UtcNow implementation for testing.
        /// By default this will be a call to <see cref="DateTime.UtcNow"/>
        /// </summary>
#if !NETSTANDARD2_0
public static Func<DateTime> UtcNow = () => DateTime.UtcNow;
#else
        public static Func<DateTime> UtcNow
        {
            get
            {
                if (_utcNow.Value == null)
                    _utcNow.Value = () => DateTime.UtcNow;
                return _utcNow.Value;
            }
            set
            {
                _utcNow.Value = value;
            }
        }
#endif

        /// <summary>
        /// Allows the setting of a custom DateTimeOffset.UtcNow implementation for testing.
        /// By default this will be a call to <see cref="DateTime.UtcNow"/>
        /// </summary>
        public static Func<DateTimeOffset> DateTimeOffsetUtcNow = () => DateTimeOffset.UtcNow;

        /// <summary>
        /// Resets the custom implementations to their defaults. 
        /// Should be called during test teardowns.
        /// </summary>
        public static void Reset()
        {

            Sleep = (timeSpan, cancellationToken) =>
            {
                if (cancellationToken.WaitHandle.WaitOne(timeSpan)) cancellationToken.ThrowIfCancellationRequested();
            };

#if NET40
            SleepAsync = TaskEx.Delay;
#else
            SleepAsync = Task.Delay;
#endif
            UtcNow = () => DateTime.UtcNow;

            DateTimeOffsetUtcNow = () => DateTimeOffset.UtcNow;
        }
    }
}