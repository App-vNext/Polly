using Microsoft.Extensions.ObjectPool;

namespace Polly;

public sealed class ResilienceContext
{
    private static readonly ObjectPool<ResilienceContext> _pool = ObjectPool.Create<ResilienceContext>();

    public ResilienceContext()
    {
    }

    public CancellationToken CancellationToken { get; set; }

    public bool IsSynchronous { get; set; }

    public Type ResultType { get; set; } = typeof(VoidResult);

    public bool IsVoid { get; set; }

    public bool ContinueOnCapturedContext { get; set; }

    public static ResilienceContext Get(CancellationToken cancellationToken = default)
    {
        var context = _pool.Get();
        context.CancellationToken = cancellationToken;
        return context;
    }

    public static void Return(ResilienceContext context)
    {
        context.Reset();

        _pool.Return(context);
    }

    private void Reset()
    {
        IsVoid = false;
        IsSynchronous = false;
        ContinueOnCapturedContext = false;
        CancellationToken = default;
    }
}
