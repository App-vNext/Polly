using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FluentAssertions;
using Polly.Builder;
using Polly.Core.Tests.Utils;
using Polly.Registry;
using Xunit;

namespace Polly.Core.Tests.Registry;
public class ResilienceStrategyRegistryTests
{
    private Action<ResilienceStrategyBuilder> _callback = _ => { };

    [Fact]
    public void Ctor_Default_Ok()
    {
        this.Invoking(_ => new ResilienceStrategyRegistry<string>()).Should().NotThrow();
    }

    [Fact]
    public void Ctor_InvalidOptions_Throws()
    {
        this.Invoking(_ => new ResilienceStrategyRegistry<string>(new ResilienceStrategyRegistryOptions<string> { BuilderFactory = null! }))
            .Should()
            .Throw<ValidationException>().WithMessage("The resilience strategy registry options are invalid.*");
    }

    [Fact]
    public void Clear_Ok()
    {
        var registry = new ResilienceStrategyRegistry<string>();

        registry.TryAddBuilder("C", (_, b) => b.AddStrategy(new TestResilienceStrategy()));

        registry.TryAdd("A", new TestResilienceStrategy());
        registry.TryAdd("B", new TestResilienceStrategy());
        registry.TryAdd("C", new TestResilienceStrategy());

        registry.Clear();

        registry.TryGet("A", out _).Should().BeFalse();
        registry.TryGet("B", out _).Should().BeFalse();
        registry.TryGet("C", out _).Should().BeTrue();
    }

    [Fact]
    public void Remove_Ok()
    {
        var registry = new ResilienceStrategyRegistry<string>();

        registry.TryAdd("A", new TestResilienceStrategy());
        registry.TryAdd("B", new TestResilienceStrategy());

        registry.Remove("A").Should().BeTrue();
        registry.Remove("A").Should().BeFalse();

        registry.TryGet("A", out _).Should().BeFalse();
        registry.TryGet("B", out _).Should().BeTrue();
    }

    [Fact]
    public void RemoveBuilder_Ok()
    {
        var registry = new ResilienceStrategyRegistry<string>();
        registry.TryAddBuilder("A", (_, b) => b.AddStrategy(new TestResilienceStrategy()));

        registry.RemoveBuilder("A").Should().BeTrue();
        registry.RemoveBuilder("A").Should().BeFalse();

        registry.TryGet("A", out _).Should().BeFalse();
    }

    [Fact]
    public void GetStrategy_BuilderMultiInstance_EnsureMultipleInstances()
    {
        var builderName = "A";
        var registry = CreateRegistry();
        var strategies = new HashSet<ResilienceStrategy>();
        registry.TryAddBuilder(StrategyId.Create(builderName), (_, builder) => builder.AddStrategy(new TestResilienceStrategy()));

        for (int i = 0; i < 100; i++)
        {
            var key = StrategyId.Create(builderName, i.ToString(CultureInfo.InvariantCulture));

            strategies.Add(registry.Get(key));

            // call again, the strategy should be already cached
            strategies.Add(registry.Get(key));
        }

        strategies.Should().HaveCount(100);
    }

    [Fact]
    public void AddBuilder_GetStrategy_EnsureCalled()
    {
        var activatorCalls = 0;
        _callback = _ => activatorCalls++;
        var registry = CreateRegistry();
        var called = 0;
        registry.TryAddBuilder(StrategyId.Create("A"), (key, builder) =>
        {
            builder.AddStrategy(new TestResilienceStrategy());
            builder.Options.Properties.Set(StrategyId.ResilienceKey, key);
            called++;
        });

        var key1 = StrategyId.Create("A");
        var key2 = StrategyId.Create("A", "Instance1");
        var key3 = StrategyId.Create("A", "Instance2");
        var keys = new[] { key1, key2, key3 };
        var strategies = keys.ToDictionary(k => k, registry.Get);
        foreach (var key in keys)
        {
            registry.Get(key);
        }

        called.Should().Be(3);
        activatorCalls.Should().Be(3);
        strategies.Keys.Should().HaveCount(3);
    }

    [Fact]
    public void TryGet_NoBuilder_Null()
    {
        var registry = CreateRegistry();
        var key = StrategyId.Create("A");

        registry.TryGet(key, out var strategy).Should().BeFalse();
        strategy.Should().BeNull();
    }

    [Fact]
    public void TryGet_ExplicitStrategyAdded_Ok()
    {
        var expectedStrategy = new TestResilienceStrategy();
        var registry = CreateRegistry();
        var key = StrategyId.Create("A", "Instance");
        registry.TryAdd(key, expectedStrategy).Should().BeTrue();

        registry.TryGet(key, out var strategy).Should().BeTrue();

        strategy.Should().BeSameAs(expectedStrategy);
    }

    [Fact]
    public void TryAdd_Twice_SecondNotAdded()
    {
        var expectedStrategy = new TestResilienceStrategy();
        var registry = CreateRegistry();
        var key = StrategyId.Create("A", "Instance");
        registry.TryAdd(key, expectedStrategy);

        registry.TryAdd(key, new TestResilienceStrategy()).Should().BeFalse();

        registry.TryGet(key, out var strategy).Should().BeTrue();
        strategy.Should().BeSameAs(expectedStrategy);
    }

    private ResilienceStrategyRegistry<StrategyId> CreateRegistry()
    {
        return new ResilienceStrategyRegistry<StrategyId>(new ResilienceStrategyRegistryOptions<StrategyId>
        {
            BuilderFactory = () =>
            {
                var builder = new ResilienceStrategyBuilder();
                _callback(builder);
                return builder;
            },
            StrategyComparer = StrategyId.Comparer,
            BuilderComparer = StrategyId.BuilderComparer
        });
    }
}
