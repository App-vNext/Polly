namespace Polly;

public abstract partial class ResilienceContextPool
{
    private sealed class SharedPool : ResilienceContextPool
    {
        private readonly ObjectPool<ResilienceContext> _pool = new(static () => new ResilienceContext(), static c => c.Reset());

        public override ResilienceContext Get(string? operationKey, CancellationToken cancellationToken = default)
        {
            var context = _pool.Get();

            context.OperationKey = operationKey;
            context.CancellationToken = cancellationToken;

            return context;
        }

        public override void Return(ResilienceContext context) => _pool.Return(Guard.NotNull(context));
    }
}
