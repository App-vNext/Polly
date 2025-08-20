using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Polly.Caching;
using Shouldly;

namespace Polly.Core.Tests.Caching
{
    public class CacheResilienceStrategyTests
    {
        [Fact]
        public async Task Miss_Caches_Then_Hit()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var options = new CacheStrategyOptions<string>
            {
                Cache = cache,
                Ttl = TimeSpan.FromMinutes(5),
                CacheKeyGenerator = _ => "key-1"
            };

            var pipeline = new ResiliencePipelineBuilder<string>()
                .AddCache(options)
                .Build();

            var r1 = await pipeline.ExecuteAsync(
                static (CancellationToken _) => new ValueTask<string>("value-1"),
                CancellationToken.None);

            cache.TryGetValue("key-1", out string? cached1).ShouldBeTrue();
            cached1.ShouldBe("value-1");

            var r2 = await pipeline.ExecuteAsync(
                static (CancellationToken _) => new ValueTask<string>("value-2"),
                CancellationToken.None);

            r1.ShouldBe("value-1");
            r2.ShouldBe("value-1");
        }

        [Fact]
        public async Task EmptyKey_Bypasses_Cache()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var options = new CacheStrategyOptions<string>
            {
                Cache = cache,
                Ttl = TimeSpan.FromMinutes(5),
                CacheKeyGenerator = _ => string.Empty
            };

            var pipeline = new ResiliencePipelineBuilder<string>()
                .AddCache(options)
                .Build();

            var r1 = await pipeline.ExecuteAsync(
                static (CancellationToken _) => new ValueTask<string>("x"),
                CancellationToken.None);

            var r2 = await pipeline.ExecuteAsync(
                static (CancellationToken _) => new ValueTask<string>("y"),
                CancellationToken.None);

            r1.ShouldBe("x");
            r2.ShouldBe("y");
        }
    }
}
