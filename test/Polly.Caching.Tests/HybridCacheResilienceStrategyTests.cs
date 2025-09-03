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
        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = null
        };

        var builder = new ResiliencePipelineBuilder<string>();
        Should.Throw<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            builder.AddHybridCache(options).Build());
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement
        using var doc1 = System.Text.Json.JsonDocument.Parse("\"cached-value\"");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string
        using var doc2 = System.Text.Json.JsonDocument.Parse("\"new-value\"");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("cached-value");
        result2.ShouldBe("cached-value");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task TypedPipeline_String_No_JsonElement_Conversion()
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

        // Execute with string values - no JsonElement conversion should happen
        var result1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("typed-value-1"));
        var result2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("typed-value-2"));

        // Both should return the cached string value
        result1.ShouldBe("typed-value-1");
        result2.ShouldBe("typed-value-1");

        // Verify no conversion happened - should still be string
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public void HybridCacheStrategyOptions_NonGeneric_CanBeInstantiated()
    {
        var options = new HybridCacheStrategyOptions();
        options.ShouldNotBeNull();
        options.ShouldBeOfType<HybridCacheStrategyOptions>();

        // Verify it inherits from the generic version
        options.ShouldBeAssignableTo<HybridCacheStrategyOptions<object>>();
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
    public async Task UntypedPipeline_JsonElement_SmallDecimal_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-small-decimal"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with very small decimal
        using var doc1 = System.Text.Json.JsonDocument.Parse("0.0000000001");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("0.0000000002");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("0.0000000001");
        result2.ShouldBe("0.0000000001");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_NonString_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-nonstring"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with non-string value
        using var doc1 = System.Text.Json.JsonDocument.Parse("42");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("99");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("42");
        result2.ShouldBe("42");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_Null_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-null"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with null value
        using var doc1 = System.Text.Json.JsonDocument.Parse("null");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("null");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached null value, not the new JsonElement
        result1.ShouldBeNull();
        result2.ShouldBeNull();

        // Verify JSON null becomes actual null
        (result1 == null).ShouldBeTrue();
        (result2 == null).ShouldBeTrue();

        // Create a test to specifically hit the JsonElement null branch
        // We'll use a separate cache key and force the scenario by caching then retrieving
        var nullTestPipeline = new ResiliencePipelineBuilder().AddHybridCache(new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "force-json-null-branch"
        }).Build();

        // Store a JsonElement null directly through our pipeline
        using var nullDoc = System.Text.Json.JsonDocument.Parse("null");
        var nullElement = nullDoc.RootElement;

        // First call stores the JsonElement null
        var nullResult1 = await nullTestPipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)nullElement));

        // Second call should hit cache and trigger conversion if JsonElement is cached
        var nullResult2 = await nullTestPipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)"should-not-execute"));

        // Verify both results are null
        nullResult1.ShouldBeNull();
        nullResult2.ShouldBeNull();
    }

    [Fact]
    public void ConvertUntypedIfJsonElement_NullJsonElement_ReturnsNull()
    {
        // Test the conversion method directly using reflection to ensure we hit the null branch
        // Now we can access it since we added InternalsVisibleTo
        var strategyType = typeof(HybridCacheResilienceStrategy<object>);
        var convertMethod = strategyType.GetMethod("ConvertUntypedIfJsonElement",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        convertMethod.ShouldNotBeNull("ConvertUntypedIfJsonElement method should exist");

        // Create a JsonElement with ValueKind.Null
        using var doc = System.Text.Json.JsonDocument.Parse("null");
        var nullElement = doc.RootElement;
        nullElement.ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);

        // Call the conversion method directly with the null JsonElement
        // The method signature is: private static TResult ConvertUntypedIfJsonElement(TResult value)
        // So for TResult = object, we pass the JsonElement as object
        var result = convertMethod.Invoke(null, new object[] { nullElement });

        // Should return null (JsonElement null gets converted to actual null)
        result.ShouldBeNull();
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_Boolean_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-bool"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with boolean value
        using var doc1 = System.Text.Json.JsonDocument.Parse("true");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("false");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("True");
        result2.ShouldBe("True");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_Object_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-obj"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with object value
        using var doc1 = System.Text.Json.JsonDocument.Parse("{\"key\":\"value\"}");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("{\"other\":\"data\"}");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("{\"key\":\"value\"}");
        result2.ShouldBe("{\"key\":\"value\"}");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_Array_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-array"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with array value
        using var doc1 = System.Text.Json.JsonDocument.Parse("[\"item1\",\"item2\"]");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("[\"item3\",\"item4\"]");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("[\"item1\",\"item2\"]");
        result2.ShouldBe("[\"item1\",\"item2\"]");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_Decimal_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-decimal"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with decimal value
        using var doc1 = System.Text.Json.JsonDocument.Parse("3.14159");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("2.71828");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("3.14159");
        result2.ShouldBe("3.14159");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task UntypedPipeline_NonJsonElement_No_Conversion_Needed()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-non-json"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores a regular object (not JsonElement)
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)"regular-string"));

        // Second execution - cache hit, should return the cached value without conversion
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)"different-string"));

        // Both should return the cached value
        result1.ShouldBe("regular-string");
        result2.ShouldBe("regular-string");

        // Verify no conversion happened - should still be string
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task TypedPipeline_String_No_JsonElement_Conversion_Path()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions<string>
        {
            Cache = cache,
            CacheKeyGenerator = _ => "typed-string-path"
        };

        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHybridCache(options)
            .Build();

        // Execute with string values - this should hit the early return path in ConvertUntypedIfJsonElement
        var result1 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("typed-value-1"));
        var result2 = await pipeline.ExecuteAsync(static _ => ValueTask.FromResult("typed-value-2"));

        // Both should return the cached string value
        result1.ShouldBe("typed-value-1");
        result2.ShouldBe("typed-value-1");

        // Verify no conversion happened - should still be string
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_StringWithSpecialChars_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-special"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with string containing special characters
        using var doc1 = System.Text.Json.JsonDocument.Parse("\"Hello\\nWorld\\t!@#$%^&*()\"");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("\"Different\\nString\\t!@#$%^&*()\"");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("Hello\nWorld\t!@#$%^&*()");
        result2.ShouldBe("Hello\nWorld\t!@#$%^&*()");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_LongString_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-long"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // Create a very long string to test edge cases
        var longString = new string('x', 10000);
        using var doc1 = System.Text.Json.JsonDocument.Parse($"\"{longString}\"");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("\"different-long-string\"");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe(longString);
        result2.ShouldBe(longString);

        // Verify the conversion actually happened by checking types and length
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
        ((string)result1).Length.ShouldBe(10000);
        ((string)result2).Length.ShouldBe(10000);
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_EmptyString_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-empty"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with empty string
        using var doc1 = System.Text.Json.JsonDocument.Parse("\"\"");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("\"non-empty\"");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("");
        result2.ShouldBe("");

        // Verify the conversion actually happened by checking types and length
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
        ((string)result1).Length.ShouldBe(0);
        ((string)result2).Length.ShouldBe(0);
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_LargeNumber_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-large-number"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with very large number
        using var doc1 = System.Text.Json.JsonDocument.Parse("9223372036854775807");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("123456789");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("9223372036854775807");
        result2.ShouldBe("9223372036854775807");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }

    [Fact]
    public async Task UntypedPipeline_JsonElement_NegativeNumber_Conversion_Covers_All_Branches()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        using var provider = services.BuildServiceProvider();

        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions
        {
            Cache = cache,
            CacheKeyGenerator = _ => "untyped-json-negative"
        };

        var pipeline = new ResiliencePipelineBuilder()
            .AddHybridCache(options)
            .Build();

        // First execution - cache miss, stores JsonElement with negative number
        using var doc1 = System.Text.Json.JsonDocument.Parse("-3.14159");
        var result1 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc1.RootElement));

        // Second execution - cache hit, should convert JsonElement to string representation
        using var doc2 = System.Text.Json.JsonDocument.Parse("2.71828");
        var result2 = await pipeline.ExecuteAsync<object>(_ => ValueTask.FromResult((object)doc2.RootElement));

        // Both should return the cached string value, not the new JsonElement
        result1.ShouldBe("-3.14159");
        result2.ShouldBe("-3.14159");

        // Verify the conversion actually happened by checking types
        result1.ShouldBeOfType<string>();
        result2.ShouldBeOfType<string>();
    }
}
