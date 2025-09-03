using System.Text.Json;
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

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "builder-key",
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        var r1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("a"));
        var r2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("b"));

        r1.ShouldBe("a");
        r2.ShouldBe("a");
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_JsonElement_Converts_To_String()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "json-key"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        using var d1 = JsonDocument.Parse("\"a\"");
        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d1.RootElement));

        using var d2 = JsonDocument.Parse("\"b\"");
        var r2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d2.RootElement));

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

    [Fact]
    public void AddHybridCache_NonGeneric_NullBuilder_Throws()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache
        };

        Should.Throw<ArgumentNullException>(() => HybridCacheResiliencePipelineBuilderExtensions.AddHybridCache(null!, options));
    }

    [Fact]
    public void AddHybridCache_Typed_NullBuilder_Throws()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache
        };

        Should.Throw<ArgumentNullException>(() => HybridCacheResiliencePipelineBuilderExtensions.AddHybridCache<string>(null!, options));
    }

    [Fact]
    public void AddHybridCache_NonGeneric_ValidOptions_BuildsSuccessfully()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "test-key"
        };

        var builder = new ResiliencePipelineBuilder();
        var result = builder.AddHybridCache(options);

        result.ShouldNotBeNull();
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddHybridCache_Typed_ValidOptions_BuildsSuccessfully()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            CacheKeyGenerator = _ => "test-key"
        };

        var builder = new ResiliencePipelineBuilder<string>();
        var result = builder.AddHybridCache(options);

        result.ShouldNotBeNull();
        result.ShouldBe(builder);
    }
}
