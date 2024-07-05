#nullable enable

namespace Polly.Caching;

/// <summary>
/// A cache policy that can be applied to the results of delegate executions.
/// </summary>
public class CachePolicy : Policy, ICachePolicy
{
    private readonly ISyncCacheProvider _syncCacheProvider;
    private readonly ITtlStrategy _ttlStrategy;
    private readonly Func<Context, string> _cacheKeyStrategy;

    private readonly Action<Context, string> _onCacheGet;
    private readonly Action<Context, string> _onCacheMiss;
    private readonly Action<Context, string> _onCachePut;
    private readonly Action<Context, string, Exception>? _onCacheGetError;
    private readonly Action<Context, string, Exception>? _onCachePutError;

    internal CachePolicy(
        ISyncCacheProvider syncCacheProvider,
        ITtlStrategy ttlStrategy,
        Func<Context, string> cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        _syncCacheProvider = syncCacheProvider;
        _ttlStrategy = ttlStrategy;
        _cacheKeyStrategy = cacheKeyStrategy;

        _onCacheGet = onCacheGet;
        _onCachePut = onCachePut;
        _onCacheMiss = onCacheMiss;
        _onCacheGetError = onCacheGetError;
        _onCachePutError = onCachePutError;
    }

    /// <inheritdoc/>
    protected override void Implementation(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        // Pass-through/NOOP policy action, for void-returning calls through a cache policy.
        action(context, cancellationToken);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return CacheEngine.Implementation<TResult>(
            _syncCacheProvider.For<TResult>(),
            _ttlStrategy.For<TResult>(),
            _cacheKeyStrategy,
            action,
            context,
            cancellationToken,
            _onCacheGet,
            _onCacheMiss,
            _onCachePut,
            _onCacheGetError,
            _onCachePutError);
    }
}

/// <summary>
/// A cache policy that can be applied to the results of delegate executions.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class CachePolicy<TResult> : Policy<TResult>, ICachePolicy<TResult>
{
    private readonly Action<Context, string> _onCacheGet;
    private readonly Action<Context, string> _onCacheMiss;
    private readonly Action<Context, string> _onCachePut;
    private readonly Action<Context, string, Exception>? _onCacheGetError;
    private readonly Action<Context, string, Exception>? _onCachePutError;

    private readonly ISyncCacheProvider<TResult> _syncCacheProvider;
    private readonly ITtlStrategy<TResult> _ttlStrategy;
    private readonly Func<Context, string> _cacheKeyStrategy;

    internal CachePolicy(
        ISyncCacheProvider<TResult> syncCacheProvider,
        ITtlStrategy<TResult> ttlStrategy,
        Func<Context, string> cacheKeyStrategy,
        Action<Context, string> onCacheGet,
        Action<Context, string> onCacheMiss,
        Action<Context, string> onCachePut,
        Action<Context, string, Exception>? onCacheGetError,
        Action<Context, string, Exception>? onCachePutError)
    {
        _syncCacheProvider = syncCacheProvider;
        _ttlStrategy = ttlStrategy;
        _cacheKeyStrategy = cacheKeyStrategy;

        _onCacheGet = onCacheGet;
        _onCachePut = onCachePut;
        _onCacheMiss = onCacheMiss;
        _onCacheGetError = onCacheGetError;
        _onCachePutError = onCachePutError;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return CacheEngine.Implementation(
            _syncCacheProvider,
            _ttlStrategy,
            _cacheKeyStrategy,
            action,
            context,
            cancellationToken,
            _onCacheGet,
            _onCacheMiss,
            _onCachePut,
            _onCacheGetError,
            _onCachePutError);
    }
}
