using System;
using System.Text.Json;
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
        _preserveComplexUntypedValues = options.PreserveComplexUntypedValues;
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var key = _keyGenerator(context) ?? string.Empty;

        // For non-generic (object) pipelines, use a wrapper to avoid JsonElement serialization issues
        if (typeof(TResult) == typeof(object))
        {
            var result = await _cache.GetOrCreateAsync<CacheObject>(
                key,
                async (_) =>
                {
                    var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
                    outcome.ThrowIfException();
                    return new CacheObject(outcome.Result!);
                },
                cancellationToken: context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);

            var normalized = _preserveComplexUntypedValues ? NormalizeComplex(result) : NormalizeValue(result.Value);
            return Outcome.FromResult((TResult)normalized!);
        }

        // For typed pipelines, use direct caching (no wrapper needed)
        var typedResult = await _cache.GetOrCreateAsync(
            key,
            async (_) =>
            {
                var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
                outcome.ThrowIfException();
                return outcome.Result!;
            },
            cancellationToken: context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);

        return Outcome.FromResult(typedResult);
    }

    // wrapper moved to top-level public type
    private readonly bool _preserveComplexUntypedValues;

    private static object? NormalizeValue(object? value)
    {
        if (value is JsonElement json)
        {
            switch (json.ValueKind)
            {
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return json.GetBoolean();
                case JsonValueKind.String:
                    return json.GetString();
                case JsonValueKind.Number:
                {
                    if (json.TryGetInt32(out var i))
                    {
                        return i;
                    }

                    if (json.TryGetInt64(out var l))
                    {
                        return l;
                    }

                    // Fallback: represent as double
                    return json.GetDouble();
                }

                default:
                {
                    return json.ToString();
                }
            }
        }

        return value;
    }

    private static object? NormalizeComplex(CacheObject wrapper)
    {
        if (wrapper.Value is null)
        {
            return null;
        }

        if (wrapper.Value is JsonElement json && !string.IsNullOrEmpty(wrapper.TypeName))
        {
            var type = Type.GetType(wrapper.TypeName!, throwOnError: false);
            if (type != null && type != typeof(JsonElement))
            {
                var restored = json.Deserialize(type);
                if (restored != null)
                {
                    return restored;
                }
            }
        }

        return NormalizeValue(wrapper.Value);
    }
}
