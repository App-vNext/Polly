namespace Polly.Utils;

internal abstract partial class CancellationTokenSourcePool
{
#if !NET8_0_OR_GREATER
    private sealed class DisposableCancellationTokenSourcePool : CancellationTokenSourcePool
    {
        private readonly TimeProvider _timeProvider;

        public DisposableCancellationTokenSourcePool(TimeProvider timeProvider) => _timeProvider = timeProvider;

        protected override CancellationTokenSource GetCore(TimeSpan delay)
        {
            if (!IsCancellable(delay))
            {
                return new CancellationTokenSource();
            }

            return _timeProvider.CreateCancellationTokenSource(delay);
        }

        public override void Return(CancellationTokenSource source) => source.Dispose();
    }
#endif
}
