namespace Polly;

public abstract partial class ResilienceContextPool
{
    private const bool ContinueOnCapturedContextDefault = false;

    private sealed class SharedPool : ResilienceContextPool
    {
        private readonly ObjectPool<ResilienceContext> _pool = new(static () => new ResilienceContext(), static c => c.Reset());

        public override ResilienceContext Get(ResilienceContextCreationArguments arguments)
        {
            var context = _pool.Get();

            context.OperationKey = arguments.OperationKey;
            context.CancellationToken = arguments.CancellationToken;
            context.ContinueOnCapturedContext = arguments.ContinueOnCapturedContext ?? ContinueOnCapturedContextDefault;

            return context;
        }

        public override void Return(ResilienceContext context) => _pool.Return(Guard.NotNull(context));
    }
}
