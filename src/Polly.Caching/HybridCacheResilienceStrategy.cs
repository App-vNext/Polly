using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Hybrid;
using Polly.Utils;

namespace Polly.Caching;

internal sealed class HybridCacheResilienceStrategy<TResult> : ResilienceStrategy<TResult>
{
    private const string EmptyKeyPlaceholder = "Polly:HybridCache:EmptyKey";

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
        if (key.Length == 0)
        {
            // Use a stable placeholder to represent an intentionally empty key
            key = EmptyKeyPlaceholder;
        }

        var result = await _cache.GetOrCreateAsync(
            key,
            (callback, context, state),
            static async (s, _) =>
            {
                var outcome = await s.callback(s.context, s.state).ConfigureAwait(s.context.ContinueOnCapturedContext);
                outcome.ThrowIfException();
                return outcome.Result!;
            },
            cancellationToken: context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);

        // Handle non-generic (object) pipelines where serializer may return JsonElement.
        return Outcome.FromResult(ConvertUntypedIfJsonElement(result));
    }

    [ExcludeFromCodeCoverage]
    private static TResult ConvertUntypedIfJsonElement(TResult value)
    {
        if (typeof(TResult) == typeof(object) && value is System.Text.Json.JsonElement json)
        {
            return (TResult)(object?)json.GetString()!;
        }

        return value;
    }
}
