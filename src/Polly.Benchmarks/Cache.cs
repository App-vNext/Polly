using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Polly.Caching;

namespace Polly.Benchmarks
{
    [Config(typeof(PollyConfig))]
    public class Cache
    {
        private static readonly MemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());
        private static readonly MemoryCacheProvider CacheProvider = new MemoryCacheProvider(MemoryCache);

        private static readonly Policy SyncPolicyMiss = Policy.Cache(CacheProvider, TimeSpan.Zero);
        private static readonly AsyncPolicy AsyncPolicyMiss = Policy.CacheAsync(CacheProvider, TimeSpan.Zero);

        private static readonly Policy SyncPolicyHit = Policy.Cache(CacheProvider, TimeSpan.MaxValue);
        private static readonly AsyncPolicy AsyncPolicyHit = Policy.CacheAsync(CacheProvider, TimeSpan.MaxValue);

        private static readonly Context HitContext = new Context(nameof(HitContext));
        private static readonly Context MissContext = new Context(nameof(MissContext));

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            SyncPolicyHit.Execute((context) => GetObject(), HitContext);
            await AsyncPolicyHit.ExecuteAsync((context, token) => GetObjectAsync(token), HitContext, CancellationToken.None);
        }

        [Benchmark]
        public object Cache_Synchronous_Hit()
        {
            return SyncPolicyHit.Execute((context) => GetObject(), HitContext);
        }

        [Benchmark]
        public async Task<object> Cache_Asynchronous_Hit()
        {
            return await AsyncPolicyHit.ExecuteAsync((context, token) => GetObjectAsync(token), HitContext, CancellationToken.None);
        }

        [Benchmark]
        public object Cache_Synchronous_Miss()
        {
            return SyncPolicyMiss.Execute((context) => GetObject(), MissContext);
        }

        [Benchmark]
        public async Task<object> Cache_Asynchronous_Miss()
        {
            return await AsyncPolicyMiss.ExecuteAsync((context, token) => GetObjectAsync(token), MissContext, CancellationToken.None);
        }

        private static object GetObject() => new object();

        private static Task<object> GetObjectAsync(CancellationToken cancellationToken) => Task.FromResult(new object());

        private sealed class MemoryCacheProvider : ISyncCacheProvider, IAsyncCacheProvider
        {
            private readonly IMemoryCache _cache;

            public MemoryCacheProvider(IMemoryCache memoryCache)
            {
                _cache = memoryCache;
            }

            public (bool, object) TryGet(string key)
            {
                bool cacheHit = _cache.TryGetValue(key, out var value);
                return (cacheHit, value);
            }

            public void Put(string key, object value, Ttl ttl)
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

            public Task<(bool, object)> TryGetAsync(string key, CancellationToken cancellationToken, bool continueOnCapturedContext)
            {
                return Task.FromResult(TryGet(key));
            }

            public Task PutAsync(string key, object value, Ttl ttl, CancellationToken cancellationToken, bool continueOnCapturedContext)
            {
                Put(key, value, ttl);
                return Task.CompletedTask;
            }
        }
    }
}
