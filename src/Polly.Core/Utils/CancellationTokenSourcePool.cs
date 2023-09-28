namespace Polly.Utils;

#pragma warning disable CA1716 // Identifiers should not match keywords

internal abstract partial class CancellationTokenSourcePool
{
    public static CancellationTokenSourcePool Create(TimeProvider timeProvider)
    {
#if NET8_0_OR_GREATER
        if (timeProvider == TimeProvider.System)
        {
            return PooledCancellationTokenSourcePool.SystemInstance;
        }

        return new PooledCancellationTokenSourcePool(timeProvider);
#elif NET6_0_OR_GREATER
        if (timeProvider == TimeProvider.System)
        {
            return PooledCancellationTokenSourcePool.SystemInstance;
        }

        return new DisposableCancellationTokenSourcePool(timeProvider);
#else
        return new DisposableCancellationTokenSourcePool(timeProvider);
#endif
    }

    public CancellationTokenSource Get(TimeSpan delay)
    {
        if (delay <= TimeSpan.Zero && delay != System.Threading.Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), delay, "Invalid delay specified.");
        }

        return GetCore(delay);
    }

    protected abstract CancellationTokenSource GetCore(TimeSpan delay);

    public abstract void Return(CancellationTokenSource source);

    protected static bool IsCancellable(TimeSpan delay) => delay != System.Threading.Timeout.InfiniteTimeSpan;
}
