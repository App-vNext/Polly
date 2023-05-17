namespace Polly.Utils;

internal abstract partial class CancellationTokenSourcePool
{
#if NET6_0_OR_GREATER
    private sealed class PooledCancellationTokenSourcePool : CancellationTokenSourcePool
    {
        public static readonly PooledCancellationTokenSourcePool SystemInstance = new(TimeProvider.System);

        public PooledCancellationTokenSourcePool(TimeProvider timeProvider) => _timeProvider = timeProvider;

        private readonly ObjectPool<CancellationTokenSource> _pool = new(
            static () => new CancellationTokenSource(),
            static cts => true);
        private readonly TimeProvider _timeProvider;

        protected override CancellationTokenSource GetCore(TimeSpan delay)
        {
            var source = _pool.Get();

            if (IsCancellable(delay))
            {
                _timeProvider.CancelAfter(source, delay);
            }

            return source;
        }

        public override void Return(CancellationTokenSource source)
        {
            if (source.TryReset())
            {
                _pool.Return(source);
            }
            else
            {
                source.Dispose();
            }
        }
    }
#endif
}
