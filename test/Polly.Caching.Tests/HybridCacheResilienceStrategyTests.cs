using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Polly;
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
    public async Task SlidingExpiration_True_Caches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            Ttl = TimeSpan.FromMinutes(5),
            UseSlidingExpiration = true,
            CacheKeyGenerator = _ => "sliding"
        };

        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHybridCache(options)
            .Build();

        var r1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("s1"), CancellationToken.None);
        var r2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("s2"), CancellationToken.None);

        r1.ShouldBe("s1");
        r2.ShouldBe("s1");
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
    public async Task EmptyKey_BypassesCache()
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

        // HybridCache bypasses caching for empty keys
        r2.ShouldBe("y");
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
    public async Task DefaultKeyGenerator_NullOperationKey_BypassesCache()
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

        // Default generator returns null -> HybridCache bypasses caching
        r1.ShouldBe("v");
        r2.ShouldBe("w");
    }

    [Fact]
    public void NullCache_Throws_ValidationException()
    {
        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = null
        };

        var builder = new ResiliencePipelineBuilder<string>();
        Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            builder.AddHybridCache(options).Build());
    }

    [Fact]
    public async Task TypedPipeline_String_Caches_Correctly()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            CacheKeyGenerator = _ => "typed-string"
        };

        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHybridCache(options)
            .Build();

        var result1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("typed-value-1"));
        var result2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("typed-value-2"));

        result1.ShouldBe("typed-value-1");
        result2.ShouldBe("typed-value-1");
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public void HybridCacheStrategyOptions_TResult_Properties_CanBeSet()
    {
        var options = new HybridCacheStrategyOptions<string>();

        // Test default values
        options.Ttl.ShouldBe(TimeSpan.FromMinutes(5));
        options.UseSlidingExpiration.ShouldBeFalse();
        options.Cache.ShouldBeNull();
        options.CacheKeyGenerator.ShouldBeNull();

        // Test setting properties
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var keyGen = new Func<Polly.ResilienceContext, string?>(ctx => "test-key");

        options.Cache = cache;
        options.Ttl = TimeSpan.FromMinutes(10);
        options.UseSlidingExpiration = true;
        options.CacheKeyGenerator = keyGen;

        options.Cache.ShouldBe(cache);
        options.Ttl.ShouldBe(TimeSpan.FromMinutes(10));
        options.UseSlidingExpiration.ShouldBeTrue();
        options.CacheKeyGenerator.ShouldBe(keyGen);
    }

    [Fact]
    public void HybridCacheStrategyOptions_TResult_Validation_Attributes_Are_Present()
    {
        var options = new HybridCacheStrategyOptions<string>();

        // Test that validation attributes work
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(options);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        // Should pass validation when Cache is set
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();
        options.Cache = cache;

        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(options, validationContext, validationResults, true);
        isValid.ShouldBeTrue();

        // Should fail validation when Cache is null
        options.Cache = null;
        isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(options, validationContext, validationResults, true);
        isValid.ShouldBeFalse();
        validationResults.ShouldContain(r => r.MemberNames.Contains("Cache"));
    }

    [Fact]
    public void HybridCacheStrategyOptions_TResult_TTL_Validation_Range_Is_Enforced()
    {
        var options = new HybridCacheStrategyOptions<string>();
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();
        options.Cache = cache;

        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(options);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        // Test valid TTL values
        options.Ttl = TimeSpan.FromSeconds(1);
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(options, validationContext, validationResults, true);
        isValid.ShouldBeTrue();

        options.Ttl = TimeSpan.FromDays(365);
        isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(options, validationContext, validationResults, true);
        isValid.ShouldBeTrue();

        // Test invalid TTL values (negative values should be invalid)
        validationResults.Clear();
        options.Ttl = TimeSpan.FromSeconds(-1);
        isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(options, validationContext, validationResults, true);
        isValid.ShouldBeFalse();
        validationResults.ShouldContain(r => r.MemberNames.Contains("Ttl"));

        validationResults.Clear();
        options.Ttl = TimeSpan.FromDays(366); // Beyond the 365 day limit
        isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(options, validationContext, validationResults, true);
        isValid.ShouldBeFalse();
        validationResults.ShouldContain(r => r.MemberNames.Contains("Ttl"));
    }

    [Fact]
    public async Task TypedPipeline_ComplexType_Caches_Correctly()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<MyComplexType>
        {
            Cache = cache,
            CacheKeyGenerator = _ => "complex-type"
        };

        var pipeline = new ResiliencePipelineBuilder<MyComplexType>()
            .AddHybridCache(options)
            .Build();

        var obj1 = new MyComplexType { Id = 1, Name = "First" };
        var obj2 = new MyComplexType { Id = 2, Name = "Second" };

        var result1 = await pipeline.ExecuteAsync(_ => ValueTask.FromResult(obj1));
        var result2 = await pipeline.ExecuteAsync(_ => ValueTask.FromResult(obj2));

        // First result should be cached
        result1.Id.ShouldBe(1);
        result1.Name.ShouldBe("First");

        // Second execution should return cached value
        result2.Id.ShouldBe(1);
        result2.Name.ShouldBe("First");
    }

    [Fact]
    public async Task TypedPipeline_IntegerType_Caches_Correctly()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<int>
        {
            Cache = cache,
            CacheKeyGenerator = _ => "int-key"
        };

        var pipeline = new ResiliencePipelineBuilder<int>()
            .AddHybridCache(options)
            .Build();

        var result1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult(42));
        var result2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult(99));

        result1.ShouldBe(42);
        result2.ShouldBe(42); // Cached value
    }

    private class MyComplexType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
