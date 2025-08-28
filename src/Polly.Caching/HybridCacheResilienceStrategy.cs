using Microsoft.Extensions.Caching.Hybrid;
using Polly.Utils;

namespace Polly.Caching;

internal sealed class HybridCacheResilienceStrategy<TResult> : ResilienceStrategy<TResult>
{
    private readonly HybridCache _cache;
    private readonly Func<ResilienceContext, string?> _keyGen;

    public HybridCacheResilienceStrategy(HybridCacheStrategyOptions<TResult> options)
    {
        Guard.NotNull(options);
        if (options.Cache is null)
        {
            throw new ArgumentException("Cache must not be null.", nameof(options));
        }

        _cache = options.Cache;
        _keyGen = options.CacheKeyGenerator ?? (ctx => ctx.OperationKey);
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var key = _keyGen(context);
        if (string.IsNullOrEmpty(key))
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var result = await _cache.GetOrCreateAsync(
            key!,
            async ct =>
            {
                var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
                outcome.ThrowIfException();
                return outcome.Result!;
            },
            cancellationToken: context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);

        return Outcome.FromResult(result);
    }
}
