#nullable enable
namespace Polly.Utilities;

// Adapted from the link below, with slight modifications.

// https://www.interact-sw.co.uk/iangblog/2004/04/26/yetmoretimedlocking
// Ian Griffiths (original TimedLock author) wrote:
// Thanks to Eric Gunnerson for recommending this be a struct rather
// than a class - avoids a heap allocation.
// Thanks to Change Gillespie and Jocelyn Coulmance for pointing out
// the bugs that then crept in when I changed it to use struct...
// Thanks to John Sands for providing the necessary incentive to make
// me invent a way of using a struct in both release and debug builds
// without losing the debug leak tracking.
internal readonly struct TimedLock : IDisposable
{
    // The TimedLock class throws a LockTimeoutException if a lock cannot be obtained within the LockTimeout.  This allows the easier discovery and debugging of deadlocks during Polly development, than if using a pure lock.
    // We do not however ever want to throw a LockTimeoutException in production - hence the forked LockTimeout value below for DEBUG versus RELEASE builds.
    // This applies particularly because CircuitBreakerPolicy runs state-change delegates during the lock, in order that the state change holds true (cannot be superseded by activity on other threads) while the delegate runs.
#if DEBUG
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);
#else
    private static readonly TimeSpan LockTimeout = TimeSpan.FromMilliseconds(int.MaxValue);
#endif

    public static TimedLock Lock(object o) =>
        Lock(o, LockTimeout);

    private static TimedLock Lock(object o, TimeSpan timeout)
    {
        var tl = new TimedLock(o);
        if (!Monitor.TryEnter(o, timeout))
        {
#if DEBUG
#pragma warning disable S3234 // "GC.SuppressFinalize" should not be invoked for types without destructors
#pragma warning disable S3971 // Do not call 'GC.SuppressFinalize'
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(tl._leakDetector);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore S3971
#pragma warning restore S3234
#endif
            throw new LockTimeoutException();
        }

        return tl;
    }
#if DEBUG
    private TimedLock(object o)
    {
        _target = o;
        _leakDetector = new Sentinel();
    }
#else
    private TimedLock(object o) => _target = o;
#endif
    private readonly object _target;

    public void Dispose()
    {
        Monitor.Exit(_target);

        // It's a bad error if someone forgets to call Dispose,
        // so in Debug builds, we put a finalizer in to detect
        // the error. If Dispose is called, we suppress the
        // finalizer.
#if DEBUG
#pragma warning disable S3234 // "GC.SuppressFinalize" should not be invoked for types without destructors
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        GC.SuppressFinalize(_leakDetector);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore S3234
#endif
    }

#if DEBUG
    // (In Debug mode, we make it a class so that we can add a finalizer
    // in order to detect when the object is not freed.)
    private sealed class Sentinel
    {
#if NETSTANDARD2_0
        ~Sentinel()
        {
            // If this finalizer runs, someone somewhere failed to
            // call Dispose, which means we've failed to leave
            // a monitor!
            Debug.Fail("Undisposed lock");
        }
#endif
    }

    private readonly Sentinel _leakDetector;
#endif
}

#pragma warning disable CA1064 // Exceptions should be public
internal sealed class LockTimeoutException : Exception
#pragma warning restore CA1064 // Exceptions should be public
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LockTimeoutException"/> class.
    /// </summary>
    public LockTimeoutException()
        : base("Timeout waiting for lock")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LockTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public LockTimeoutException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LockTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public LockTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
