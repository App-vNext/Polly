using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Polly.Caching;
using Polly.Testing;
using Shouldly;

namespace Polly.Caching.Tests;

public class HybridCacheResiliencePipelineBuilderExtensionsTests
{
    [Fact]
    public void NonGeneric_AddHybridCache_BuildsStrategy()
    {
        var services = new ServiceCollection().AddHybridCache();
        using var provider = services.Services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var options = new HybridCacheStrategyOptions { Cache = cache };

        var builder = new ResiliencePipelineBuilder()
            .AddHybridCache(options);

        builder.Build()
            .GetPipelineDescriptor()
            .FirstStrategy
            .StrategyInstance
            .ShouldBeOfType<HybridCacheResilienceStrategy<object>>();
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
