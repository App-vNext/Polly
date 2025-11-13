using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;

namespace Polly.Caching;

internal sealed class HybridCacheResilienceStrategy<TResult> : ResilienceStrategy<TResult>
{
    private readonly HybridCache _cache;
    private readonly Func<ResilienceContext, string?> _keyGenerator;

    public HybridCacheResilienceStrategy(HybridCacheStrategyOptions<TResult> options)
    {
        Guard.NotNull(options);
        _cache = options.Cache!;
        _keyGenerator = options.CacheKeyGenerator ?? (static ctx => ctx.OperationKey);
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var key = _keyGenerator(context) ?? string.Empty;

        var result = await _cache.GetOrCreateAsync(
            key,
            async (_) =>
            {
                var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
                outcome.ThrowIfException();
                return outcome.Result!;
            },
            cancellationToken: context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);

        return Outcome.FromResult(result);
    }
}
