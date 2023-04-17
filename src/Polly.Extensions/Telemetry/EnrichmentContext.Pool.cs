using Polly.Strategy;
using Polly.Utils;

namespace Polly.Extensions.Telemetry;

public partial class EnrichmentContext
{
    private static readonly ObjectPool<EnrichmentContext> ContextPool = new(
        static () => new EnrichmentContext(),
        static context =>
        {
            context.Outcome = null;
            context.ResilienceContext = null!;
            context.Tags.Clear();
            return true;
        });

    internal static EnrichmentContext Get(IResilienceArguments arguments, Outcome? outcome)
    {
        var context = ContextPool.Get();
        context.ResilienceContext = arguments.Context;
        context.ResilienceArguments = arguments;
        context.Outcome = outcome;

        return context;
    }

    internal static void Return(EnrichmentContext context)
    {
        context.Tags.Clear();
        ContextPool.Return(context);
    }
}
