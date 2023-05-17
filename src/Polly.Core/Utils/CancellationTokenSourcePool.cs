namespace Polly.Utils;

#pragma warning disable CA1716 // Identifiers should not match keywords

internal abstract partial class CancellationTokenSourcePool
{
    public static CancellationTokenSourcePool Create(TimeProvider timeProvider)
    {
#if NET6_0_OR_GREATER
        if (timeProvider == TimeProvider.System)
        {
            return PooledCancellationTokenSourcePool.SystemInstance;
        }
#endif
        return new DisposableCancellationTokenSourcePool(timeProvider);
    }

    public abstract CancellationTokenSource Get(TimeSpan delay);

    public abstract void Return(CancellationTokenSource source);

    protected static bool IsCancellable(TimeSpan delay) => delay > TimeSpan.Zero;
}
