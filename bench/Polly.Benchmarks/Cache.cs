using Microsoft.Extensions.Caching.Memory;

namespace Polly.Benchmarks;

[Config(typeof(PollyConfig))]
public class Cache
{
    private static readonly MemoryCache MemoryCache = new(new MemoryCacheOptions());
    private static readonly MemoryCacheProvider CacheProvider = new(MemoryCache);

    private static readonly Policy SyncPolicyMiss = Policy.Cache(CacheProvider, TimeSpan.Zero);
    private static readonly AsyncPolicy AsyncPolicyMiss = Policy.CacheAsync(CacheProvider, TimeSpan.Zero);

    private static readonly Policy SyncPolicyHit = Policy.Cache(CacheProvider, TimeSpan.MaxValue);
    private static readonly AsyncPolicy AsyncPolicyHit = Policy.CacheAsync(CacheProvider, TimeSpan.MaxValue);

    private static readonly Context HitContext = [with(nameof(HitContext))];
    private static readonly Context MissContext = [with(nameof(MissContext))];

    [GlobalSetup]
    public Task GlobalSetup()
    {
        SyncPolicyHit.Execute(_ => GetObject(), HitContext);
        return AsyncPolicyHit.ExecuteAsync((_, token) => GetObjectAsync(token), HitContext, CancellationToken.None);
    }

    [Benchmark]
    public object Cache_Synchronous_Hit() =>
        SyncPolicyHit.Execute(_ => GetObject(), HitContext);

    [Benchmark]
    public Task<object> Cache_Asynchronous_Hit() =>
        AsyncPolicyHit.ExecuteAsync((_, token) => GetObjectAsync(token), HitContext, CancellationToken.None);

    [Benchmark]
    public object Cache_Synchronous_Miss() =>
        SyncPolicyMiss.Execute(_ => GetObject(), MissContext);

    [Benchmark]
    public Task<object> Cache_Asynchronous_Miss() =>
        AsyncPolicyMiss.ExecuteAsync((_, token) => GetObjectAsync(token), MissContext, CancellationToken.None);

    private static object GetObject() => new();

    private static Task<object> GetObjectAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new object());

    private sealed class MemoryCacheProvider(IMemoryCache memoryCache) : ISyncCacheProvider, IAsyncCacheProvider
    {
        private readonly IMemoryCache _cache = memoryCache;

        public (bool, object?) TryGet(string key)
        {
            bool cacheHit = _cache.TryGetValue(key, out var value);
            return (cacheHit, value);
        }

        public void Put(string key, object? value, Ttl ttl)
        {
            TimeSpan remaining = DateTimeOffset.MaxValue - DateTimeOffset.UtcNow;
            var options = new MemoryCacheEntryOptions();

            if (ttl.SlidingExpiration)
            {
                options.SlidingExpiration = ttl.Timespan < remaining ? ttl.Timespan : remaining;
            }
            else
            {
                if (ttl.Timespan == TimeSpan.MaxValue)
                {
                    options.AbsoluteExpiration = DateTimeOffset.MaxValue;
                }
                else
                {
                    options.AbsoluteExpirationRelativeToNow = ttl.Timespan < remaining ? ttl.Timespan : remaining;
                }
            }

            _cache.Set(key, value, options);
        }

        public Task<(bool, object?)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
            Task.FromResult(TryGet(key));

        public Task PutAsync(string key, object? value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            Put(key, value, ttl);
            return Task.CompletedTask;
        }
    }
}
