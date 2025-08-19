using Microsoft.Extensions.Caching.Memory;
using Polly.Utils;

namespace Polly.Caching;

internal sealed class CacheResilienceStrategy<TResult> : ResilienceStrategy<TResult>
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl;
    private readonly bool _useSliding;
    private readonly Func<ResilienceContext, string?> _keyGen;

    public CacheResilienceStrategy(CacheStrategyOptions<TResult> options)
    {
        _cache = options.Cache ?? throw new ArgumentNullException(nameof(options.Cache));
        _ttl = options.Ttl;
        _useSliding = options.UseSlidingExpiration;
        _keyGen = options.CacheKeyGenerator ?? (ctx => ctx.OperationKey);
    }

    protected internal override async ValueTask<Outcome<TResult>> ExecuteCore<TState>(
	Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
	ResilienceContext context,
	TState state)
    {
        var key = _keyGen(context);
        if (string.IsNullOrEmpty(key))
        {
            return await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state)
                .ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var cacheKey = key!;

        if (_cache.TryGetValue(cacheKey, out TResult? cached))
        {
            return Outcome.FromResult(cached!);
        }

        var outcome = await StrategyHelper.ExecuteCallbackSafeAsync(callback, context, state)
            .ConfigureAwait(context.ContinueOnCapturedContext);

        if (outcome.Exception is null)
        {
            var options = new MemoryCacheEntryOptions();
            if (_useSliding)
            {
                options.SlidingExpiration = _ttl;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = _ttl;
            }

            var value = outcome.GetResultOrRethrow();
            _cache.Set(cacheKey, value, options);
        }

        return outcome;
    }
}
