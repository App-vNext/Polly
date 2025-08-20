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
        Guard.NotNull(options);
        if (options.Cache is null)
        {
            throw new ArgumentException("Cache must not be null.", nameof(options));
        }

        _cache = options.Cache;
        _ttl = options.Ttl;
        _useSliding = options.UseSlidingExpiration;
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

        var cacheKey = key!;

        if (_cache.TryGetValue(cacheKey, out TResult? cached))
        {
            return Outcome.FromResult(cached!);
        }

        var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

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

            outcome.ThrowIfException();
            var value = outcome.Result!;
            _cache.Set(cacheKey, value, options);
        }

        return outcome;
    }
}
