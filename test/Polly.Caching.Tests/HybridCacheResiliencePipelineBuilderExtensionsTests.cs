using System.Linq;
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
    public async Task NonGeneric_AddHybridCache_BuildsAndCaches_ValueType()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "builder-key"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        var r1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult(1));
        var r2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult(2));

        r1.ShouldBe(1);
        r2.ShouldBe(1);
    }

    [Fact]
    public void AddHybridCache_NonGeneric_NullOptions_Throws()
    {
        var builder = new ResiliencePipelineBuilder();
        Should.Throw<ArgumentNullException>(() => builder.AddHybridCache(options: null!));
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_BuildsAndCaches_IntJsonElement()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "int-json"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        using var d1 = JsonDocument.Parse("123");
        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d1.RootElement));

        using var d2 = JsonDocument.Parse("456");
        var r2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d2.RootElement));

        r1.ShouldBe(123);
        r2.ShouldBe(123);
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_BuildsAndCaches_DoubleJsonElement()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "double-json"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        using var d1 = JsonDocument.Parse("3.14");
        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d1.RootElement));

        using var d2 = JsonDocument.Parse("2.71");
        var r2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d2.RootElement));

        r1.ShouldBe(3.14);
        r2.ShouldBe(3.14);
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_BuildsAndCaches_BoolJsonElement()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "bool-json"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        using var d1 = JsonDocument.Parse("true");
        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d1.RootElement));

        r1.ShouldBe(true);
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_BuildsAndCaches_NullJsonElement()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "null-json"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        using var d1 = JsonDocument.Parse("null");
        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d1.RootElement));

        r1.ShouldBeNull();
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_BuildsAndCaches_ObjectJsonElement_ToString()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "object-json"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        using var d1 = JsonDocument.Parse("{\"a\":1}");
        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d1.RootElement));

        r1.ShouldBe("{\"a\":1}");
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_BuildsAndCaches_NullJsonElement_ThroughWrapper()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "json-null-through-wrapper"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        using var d1 = JsonDocument.Parse("null");
        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d1.RootElement));

        r1.ShouldBeNull();
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

    [Fact]
    public async Task NonGeneric_AddHybridCache_For_Custom_Object_WithOptIn_Preserves_Type()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "custom-object-key",
            PreserveComplexUntypedValues = true,
        };

        var bandit = new Dog { Name = "Bandit" };
        var chilli = new Dog { Name = "Chilli" };
        var bluey = new Dog { Name = "Bluey", Father = bandit, Mother = chilli };
        var bingo = new Dog { Name = "Bingo", Father = bandit, Mother = chilli };

        var family = new List<Dog> { bandit, bingo, bluey, chilli };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        var r1 = await pipeline.ExecuteAsync(_ => ValueTask.FromResult((object)family));
        var r2 = await pipeline.ExecuteAsync(_ => ValueTask.FromResult((object)family));

        r1.ShouldBeOfType<List<Dog>>();
        r2.ShouldBeOfType<List<Dog>>();

        // Validate content equality (order and names)
        var names1 = ((List<Dog>)r1).Select(d => d.Name).ToArray();
        var names2 = ((List<Dog>)r2).Select(d => d.Name).ToArray();
        names1.ShouldBe(new[] { "Bandit", "Bingo", "Bluey", "Chilli" });
        names2.ShouldBe(new[] { "Bandit", "Bingo", "Bluey", "Chilli" });
    }

    private sealed class Dog
    {
        public required string Name { get; init; }

        public Dog? Father { get; set; }

        public Dog? Mother { get; set; }
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_BoolJsonElement_WithOptIn()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "bool-json-optin",
            PreserveComplexUntypedValues = true,
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        using var d1 = JsonDocument.Parse("true");
        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d1.RootElement));

        r1.ShouldBe(true);
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_ObjectJsonElement_WithOptIn_ToString()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "obj-json-optin",
            PreserveComplexUntypedValues = true,
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        using var d1 = JsonDocument.Parse("{\"a\":1}");
        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)d1.RootElement));

        r1.ShouldBe("{\"a\":1}");
    }

    [Fact]
    public async Task NonGeneric_AddHybridCache_NullValue_WithOptIn_ReturnsNull()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "null-optin",
            PreserveComplexUntypedValues = true,
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        var r1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)null!));
        r1.ShouldBeNull();
    }
}
