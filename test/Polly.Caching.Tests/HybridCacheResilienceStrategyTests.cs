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

        var r1 = await pipeline.ExecuteAsync(
            (Func<CancellationToken, ValueTask<string>>)(static _ => new("v1")),
            CancellationToken.None);

        var r2 = await pipeline.ExecuteAsync(
            (Func<CancellationToken, ValueTask<string>>)(static _ => new("v2")),
            CancellationToken.None);

        r1.ShouldBe("v1");
        r2.ShouldBe("v1");
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
                (Func<CancellationToken, ValueTask<string>>)(static _ =>
                    new(Task.FromException<string>(new InvalidOperationException("boom")))),
                CancellationToken.None);
        });
    }

    [Fact]
    public async Task DefaultKeyGenerator_NullOperationKey_Throws()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            CacheKeyGenerator = null
        };

        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHybridCache(options)
            .Build();

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            _ = await pipeline.ExecuteAsync(
                (Func<CancellationToken, ValueTask<string>>)(static _ => new("v")),
                CancellationToken.None);
        });
    }

    [Fact]
    public void NullCache_Throws_ValidationException()
    {
        var options = new HybridCacheStrategyOptions<string> { Cache = null };

        var builder = new ResiliencePipelineBuilder<string>();
        Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            builder.AddHybridCache(options).Build());
    }

    [Fact]
    public void StrategyCtor_NullCache_Throws_ArgumentException()
    {
        // Directly construct the internal strategy to bypass builder validation
        var options = new HybridCacheStrategyOptions<string>(); // Cache is null by default
        Should.Throw<ArgumentException>(() => new HybridCacheResilienceStrategy<string>(options));
    }
}
