namespace Polly.Utils
{
    internal static class CancellationTokenSourcePool
    {
#if NET6_0_OR_GREATER
        private static readonly ObjectPool<CancellationTokenSource> Pool = new(
            static () => new CancellationTokenSource(),
            static cts => true);
#endif
        public static CancellationTokenSource Get()
        {
#if NET6_0_OR_GREATER
            return Pool.Get();
#else
            return new CancellationTokenSource();
#endif
        }

        public static void Return(CancellationTokenSource source)
        {
#if NET6_0_OR_GREATER
            if (source.TryReset())
            {
                Pool.Return(source);
                return;
            }
#endif
            source.Dispose();
        }
    }
}
