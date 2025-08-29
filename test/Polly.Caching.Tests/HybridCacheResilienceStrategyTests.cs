using System.Threading;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Polly.Caching;
using Shouldly;

namespace Polly.Caching.Tests;

public class HybridCacheResilienceStrategyTests
{
    [Fact]
    public async Task Miss_Caches_Then_Hit()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            Ttl = TimeSpan.FromMinutes(5),
            CacheKeyGenerator = _ => "key-1"
        };

        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHybridCache(options)
            .Build();

        var r1 = await pipeline.ExecuteAsync(
            (Func<CancellationToken, ValueTask<string>>)(static _ => new("value-1")),
            CancellationToken.None);

        var r2 = await pipeline.ExecuteAsync(
            (Func<CancellationToken, ValueTask<string>>)(static _ => new("value-2")),
            CancellationToken.None);

        r1.ShouldBe("value-1");
        r2.ShouldBe("value-1");
    }

    [Fact]
    public async Task EmptyKey_Throws()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            Ttl = TimeSpan.FromMinutes(5),
            CacheKeyGenerator = _ => string.Empty
        };

        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHybridCache(options)
            .Build();

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            _ = await pipeline.ExecuteAsync(
                (Func<CancellationToken, ValueTask<string>>)(static _ => new("x")),
                CancellationToken.None);
        });
    }
}
