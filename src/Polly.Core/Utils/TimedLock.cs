using Polly;

namespace Polly.Utils;

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
#pragma warning disable S3971 // "GC.SuppressFinalize" should not be called
#pragma warning disable CA1821 // Remove empty Finalizers
#pragma warning disable IDE0021 // Use expression body for constructor
#pragma warning disable IDE0022 // Use expression body for method

// Adapted from the link below, with slight modifications.

// http://www.interact-sw.co.uk/iangblog/2004/04/26/yetmoretimedlocking
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
    private readonly object _target;

    private TimedLock(object o)
    {
        _target = o;
#if DEBUG
        _leakDetector = new Sentinel();
#endif
    }

    // The TimedLock class throws a InvalidOperationException if a lock cannot be obtained within the LockTimeout.
    // This allows the easier discovery and debugging of deadlocks during Polly development, than if using a pure lock.
    // We do not however ever want to throw a InvalidOperationException in production - hence the forked LockTimeout value below for DEBUG versus RELEASE builds.
    // This applies particularly because CircuitBreakerPolicy runs state-change delegates during the lock,
    // in order that the state change holds true (cannot be superseded by activity on other threads) while the delegate runs.

#if DEBUG
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);

    public static TimedLock Lock(object o)
    {
        var tl = new TimedLock(o);
        if (!Monitor.TryEnter(o, LockTimeout))
        {
            GC.SuppressFinalize(tl._leakDetector);
            throw new InvalidOperationException("Timeout waiting for lock.");
        }

        return tl;
    }
#else
    public static TimedLock Lock(object o)
    {
        var tl = new TimedLock(o);
        Monitor.Enter(o);
        return tl;
    }
#endif

    public void Dispose()
    {
        Monitor.Exit(_target);

        // It's a bad error if someone forgets to call Dispose,
        // so in Debug builds, we put a finalizer in to detect
        // the error. If Dispose is called, we suppress the
        // finalizer.
#if DEBUG
        GC.SuppressFinalize(_leakDetector);
#endif
    }

#if DEBUG
    // (In Debug mode, we make it a class so that we can add a finalizer
    // in order to detect when the object is not freed.)
    private class Sentinel
    {
        ~Sentinel()
        {
            // If this finalizer runs, someone somewhere failed to
            // call Dispose, which means we've failed to leave
            // a monitor!
            Debug.Fail("Undisposed lock");
        }
    }

    private readonly Sentinel _leakDetector;
#endif
}

