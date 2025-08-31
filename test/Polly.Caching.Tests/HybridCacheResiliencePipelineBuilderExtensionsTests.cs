using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Polly.Caching;
using Shouldly;

namespace Polly.Caching.Tests;

public class HybridCacheResiliencePipelineBuilderExtensionsTests
{
    [Fact]
    public async Task NonGeneric_AddHybridCache_BuildsAndCaches()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions { Cache = cache, CacheKeyGenerator = _ => "builder-key" };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        var r1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("a"));
        var r2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("b"));

        r1.ShouldBe("a");
        r2.ShouldBe("a");
    }

    [Fact]
    public void AddHybridCache_NonGeneric_NullOptions_Throws()
    {
        var builder = new ResiliencePipelineBuilder();
        Should.Throw<ArgumentNullException>(() => builder.AddHybridCache(options: null!));
    }

    [Fact]
    public void AddHybridCache_Typed_NullOptions_Throws()
    {
        var builder = new ResiliencePipelineBuilder<string>();
        Should.Throw<ArgumentNullException>(() => builder.AddHybridCache(options: null!));
    }
}
