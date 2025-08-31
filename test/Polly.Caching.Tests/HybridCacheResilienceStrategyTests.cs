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

        var r1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("value-1"), CancellationToken.None);
        var r2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("value-2"), CancellationToken.None);

        r1.ShouldBe("value-1");
        r2.ShouldBe("value-1");
    }

    [Fact]
    public async Task KeyGenerator_Uses_Returned_Key_And_Caches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            CacheKeyGenerator = _ => "op-key"
        };

        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHybridCache(options)
            .Build();

        var r1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("v1"), CancellationToken.None);
        var r2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("v2"), CancellationToken.None);

        r1.ShouldBe("v1");
        r2.ShouldBe("v1");
    }

    [Fact]
    public async Task EmptyKey_Caches()
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

        var r1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("x"), CancellationToken.None);
        var r2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("y"), CancellationToken.None);

        r1.ShouldBe("x");
        r2.ShouldBe("x");
    }

    [Fact]
    public void Options_Defaults_Are_Documented()
    {
        var options = new HybridCacheStrategyOptions<string>();
        options.Ttl.ShouldBe(TimeSpan.FromMinutes(5));
        options.UseSlidingExpiration.ShouldBeFalse();
    }

    [Fact]
    public async Task ValueFactory_Propagates_Callback_Exception()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            CacheKeyGenerator = _ => "err-key"
        };

        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHybridCache(options)
            .Build();

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            _ = await pipeline.ExecuteAsync(
                static _ => new ValueTask<string>(Task.FromException<string>(new InvalidOperationException("boom"))),
                CancellationToken.None);
        });
    }

    [Fact]
    public async Task DefaultKeyGenerator_NullOperationKey_Caches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            CacheKeyGenerator = null // default generator -> OperationKey (null)
        };

        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHybridCache(options)
            .Build();

        var r1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("v"), CancellationToken.None);
        var r2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("w"), CancellationToken.None);

        r1.ShouldBe("v");
        r2.ShouldBe("v");
    }

    [Fact]
    public void NullCache_Throws_ValidationException()
    {
        var options = new HybridCacheStrategyOptions<string> { Cache = null };

        var builder = new ResiliencePipelineBuilder<string>();
        Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            builder.AddHybridCache(options).Build());
    }
}
