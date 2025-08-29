using Microsoft.Extensions.Caching.Hybrid;
using Polly.Utils;

namespace Polly.Caching;

internal sealed class HybridCacheResilienceStrategy<TResult> : ResilienceStrategy<TResult>
{
    private readonly HybridCache _cache;
    private readonly Func<ResilienceContext, string?> _keyGenerator;

    public HybridCacheResilienceStrategy(HybridCacheStrategyOptions<TResult> options)
    {
        Guard.NotNull(options);
        if (options.Cache is null)
        {
            throw new ArgumentException("Cache must not be null.", nameof(options));
        }

        _cache = options.Cache;
        _keyGenerator = options.CacheKeyGenerator ?? (static ctx => ctx.OperationKey);
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var key = _keyGenerator(context);
        if (string.IsNullOrEmpty(key))
        {
            return Outcome.FromException<TResult>(new InvalidOperationException("HybridCache key was null or empty."));
        }

        var payload = new FactoryState<TState>(callback, context, state, context.ContinueOnCapturedContext);

        var result = await _cache.GetOrCreateAsync(
            key,
            payload,
            static async (s, _) =>
            {
                var outcome = await s.Callback(s.Context, s.State).ConfigureAwait(s.ContinueOnCapturedContext);
                outcome.ThrowIfException();
                return outcome.Result!;
            },
            cancellationToken: context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);

        return Outcome.FromResult(result);
    }

    private readonly struct FactoryState<T>
    {
        public FactoryState(
            Func<ResilienceContext, T, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            T state,
            bool continueOnCapturedContext)
        {
            Callback = callback;
            Context = context;
            State = state;
            ContinueOnCapturedContext = continueOnCapturedContext;
        }

        public Func<ResilienceContext, T, ValueTask<Outcome<TResult>>> Callback { get; }
        public ResilienceContext Context { get; }
        public T State { get; }
        public bool ContinueOnCapturedContext { get; }
    }
}
