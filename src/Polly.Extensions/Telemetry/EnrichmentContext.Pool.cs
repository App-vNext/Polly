using Polly.Utils;

namespace Polly.Extensions.Telemetry;

public partial class EnrichmentContext
{
    private static readonly ObjectPool<EnrichmentContext> ContextPool = new(
        static () => new EnrichmentContext(),
        static context =>
        {
            context.Outcome = null;
            context.Context = null!;
            context.Tags.Clear();
            return true;
        });

    internal static EnrichmentContext Get(ResilienceContext resilienceContext, object? arguments, Outcome<object>? outcome)
    {
        var context = ContextPool.Get();
        context.Context = resilienceContext;
        context.Arguments = arguments;
        context.Outcome = outcome;

        return context;
    }

    internal static void Return(EnrichmentContext context)
    {
        context.Tags.Clear();
        ContextPool.Return(context);
    }
}
