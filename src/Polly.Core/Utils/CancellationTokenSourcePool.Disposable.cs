namespace Polly.Utils;

internal abstract partial class CancellationTokenSourcePool
{
    private sealed class DisposableCancellationTokenSourcePool : CancellationTokenSourcePool
    {
        private readonly TimeProvider _timeProvider;

        public DisposableCancellationTokenSourcePool(TimeProvider timeProvider) => _timeProvider = timeProvider;

        public override CancellationTokenSource Get(TimeSpan delay)
        {
            var source = new CancellationTokenSource();

            if (IsCancellable(delay))
            {
                _timeProvider.CancelAfter(source, delay);
            }

            return source;
        }

        public override void Return(CancellationTokenSource source) => source.Dispose();
    }
}
