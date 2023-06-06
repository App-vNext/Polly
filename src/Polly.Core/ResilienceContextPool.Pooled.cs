namespace Polly;

public abstract partial class ResilienceContextPool
{
    private sealed class Pooled : ResilienceContextPool
    {
        private readonly ObjectPool<ResilienceContext> _pool = new(static () => new ResilienceContext(), static c => c.Reset());

        public override ResilienceContext Get() => _pool.Get();

        public override void Return(ResilienceContext context)
        {
            Guard.NotNull(context);

            _pool.Return(context);
        }
    }
}
