using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Polly.Registry;
using Polly.Telemetry;

namespace Polly.Core.Tests.Registry;

public class ResilienceStrategyRegistryTests
{
    private readonly ResilienceStrategyRegistryOptions<StrategyId> _options;

    private Action<ResilienceStrategyBuilder> _callback = _ => { };

    public ResilienceStrategyRegistryTests() => _options = new()
    {
        BuilderFactory = () =>
        {
            var builder = new ResilienceStrategyBuilder();
            _callback(builder);
            return builder;
        },
        StrategyComparer = StrategyId.Comparer,
        BuilderComparer = StrategyId.BuilderComparer
    };

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

        registry.TryAddBuilder("C", (b, _) => b.AddStrategy(new TestResilienceStrategy()));

        registry.TryAdd("A", new TestResilienceStrategy());
        registry.TryAdd("B", new TestResilienceStrategy());
        registry.TryAdd("C", new TestResilienceStrategy());

        registry.Clear();

        registry.TryGet("A", out _).Should().BeFalse();
        registry.TryGet("B", out _).Should().BeFalse();
        registry.TryGet("C", out _).Should().BeTrue();
    }

    [Fact]
    public void Clear_Generic_Ok()
    {
        var registry = new ResilienceStrategyRegistry<string>();

        registry.TryAddBuilder<string>("C", (b, _) => b.AddStrategy(new TestResilienceStrategy()));

        registry.TryAdd("A", new TestResilienceStrategy<string>());
        registry.TryAdd("B", new TestResilienceStrategy<string>());
        registry.TryAdd("C", new TestResilienceStrategy<string>());

        registry.Clear<string>();

        registry.TryGet<string>("A", out _).Should().BeFalse();
        registry.TryGet<string>("B", out _).Should().BeFalse();
        registry.TryGet<string>("C", out _).Should().BeTrue();
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
    public void Remove_Generic_Ok()
    {
        var registry = new ResilienceStrategyRegistry<string>();

        registry.TryAdd("A", new TestResilienceStrategy<string>());
        registry.TryAdd("B", new TestResilienceStrategy<string>());

        registry.Remove<string>("A").Should().BeTrue();
        registry.Remove<string>("A").Should().BeFalse();

        registry.TryGet<string>("A", out _).Should().BeFalse();
        registry.TryGet<string>("B", out _).Should().BeTrue();
    }

    [Fact]
    public void RemoveBuilder_Ok()
    {
        var registry = new ResilienceStrategyRegistry<string>();
        registry.TryAddBuilder("A", (b, _) => b.AddStrategy(new TestResilienceStrategy()));

        registry.RemoveBuilder("A").Should().BeTrue();
        registry.RemoveBuilder("A").Should().BeFalse();

        registry.TryGet("A", out _).Should().BeFalse();
    }

    [Fact]
    public void RemoveBuilder_Generic_Ok()
    {
        var registry = new ResilienceStrategyRegistry<string>();
        registry.TryAddBuilder<string>("A", (b, _) => b.AddStrategy(new TestResilienceStrategy()));

        registry.RemoveBuilder<string>("A").Should().BeTrue();
        registry.RemoveBuilder<string>("A").Should().BeFalse();

        registry.TryGet<string>("A", out _).Should().BeFalse();
    }

    [Fact]
    public void GetStrategy_BuilderMultiInstance_EnsureMultipleInstances()
    {
        var builderName = "A";
        var registry = CreateRegistry();
        var strategies = new HashSet<ResilienceStrategy>();
        registry.TryAddBuilder(StrategyId.Create(builderName), (builder, _) => builder.AddStrategy(new TestResilienceStrategy()));

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
    public void GetStrategy_GenericBuilderMultiInstance_EnsureMultipleInstances()
    {
        var builderName = "A";
        var registry = CreateRegistry();
        var strategies = new HashSet<ResilienceStrategy<string>>();
        registry.TryAddBuilder<string>(StrategyId.Create(builderName), (builder, _) => builder.AddStrategy(new TestResilienceStrategy()));

        for (int i = 0; i < 100; i++)
        {
            var key = StrategyId.Create(builderName, i.ToString(CultureInfo.InvariantCulture));

            strategies.Add(registry.Get<string>(key));

            // call again, the strategy should be already cached
            strategies.Add(registry.Get<string>(key));
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
        registry.TryAddBuilder(StrategyId.Create("A"), (builder, context) =>
        {
            builder.AddStrategy(new TestResilienceStrategy());
            builder.Properties.Set(StrategyId.ResilienceKey, context.StrategyKey);
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
    public void AddBuilder_GenericGetStrategy_EnsureCalled()
    {
        var activatorCalls = 0;
        _callback = _ => activatorCalls++;
        var registry = CreateRegistry();
        var called = 0;
        registry.TryAddBuilder<string>(StrategyId.Create("A"), (builder, context) =>
        {
            builder.AddStrategy(new TestResilienceStrategy());
            builder.Properties.Set(StrategyId.ResilienceKey, context.StrategyKey);
            called++;
        });

        var key1 = StrategyId.Create("A");
        var key2 = StrategyId.Create("A", "Instance1");
        var key3 = StrategyId.Create("A", "Instance2");
        var keys = new[] { key1, key2, key3 };
        var strategies = keys.ToDictionary(k => k, registry.Get<string>);
        foreach (var key in keys)
        {
            registry.Get<string>(key);
        }

        called.Should().Be(3);
        activatorCalls.Should().Be(3);
        strategies.Keys.Should().HaveCount(3);
    }

    [Fact]
    public void AddBuilder_EnsureStrategyKey()
    {
        _options.BuilderNameFormatter = k => k.BuilderName;
        _options.StrategyKeyFormatter = k => k.InstanceName;

        var called = false;
        var registry = CreateRegistry();
        registry.TryAddBuilder(StrategyId.Create("A"), (builder, context) =>
        {
            context.BuilderName.Should().Be("A");
            context.StrategyKeyString.Should().Be("Instance1");
            builder.AddStrategy(new TestResilienceStrategy());
            builder.BuilderName.Should().Be("A");
            builder.Properties.TryGetValue(TelemetryUtil.StrategyKey, out var val).Should().BeTrue();
            val.Should().Be("Instance1");
            called = true;
        });

        registry.Get(StrategyId.Create("A", "Instance1"));
        called.Should().BeTrue();
    }

    [Fact]
    public void AddBuilder_MultipleGeneric_EnsureDistinctInstances()
    {
        var registry = CreateRegistry();
        registry.TryAddBuilder<string>(StrategyId.Create("A"), (builder, _) => builder.AddStrategy(new TestResilienceStrategy()));
        registry.TryAddBuilder<int>(StrategyId.Create("A"), (builder, _) => builder.AddStrategy(new TestResilienceStrategy()));

        registry.Get<string>(StrategyId.Create("A", "Instance1")).Should().BeSameAs(registry.Get<string>(StrategyId.Create("A", "Instance1")));
        registry.Get<int>(StrategyId.Create("A", "Instance1")).Should().BeSameAs(registry.Get<int>(StrategyId.Create("A", "Instance1")));
    }

    [Fact]
    public void AddBuilder_Generic_EnsureStrategyKey()
    {
        _options.BuilderNameFormatter = k => k.BuilderName;
        _options.StrategyKeyFormatter = k => k.InstanceName;

        var called = false;
        var registry = CreateRegistry();
        registry.TryAddBuilder<string>(StrategyId.Create("A"), (builder, _) =>
        {
            builder.AddStrategy(new TestResilienceStrategy());
            builder.BuilderName.Should().Be("A");
            builder.Properties.TryGetValue(TelemetryUtil.StrategyKey, out var val).Should().BeTrue();
            val.Should().Be("Instance1");
            called = true;
        });

        registry.Get<string>(StrategyId.Create("A", "Instance1"));
        called.Should().BeTrue();
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
    public void TryGet_GenericNoBuilder_Null()
    {
        var registry = CreateRegistry();
        var key = StrategyId.Create("A");

        registry.TryGet<string>(key, out var strategy).Should().BeFalse();
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
    public void TryGet_GenericExplicitStrategyAdded_Ok()
    {
        var expectedStrategy = new TestResilienceStrategy<string>();
        var registry = CreateRegistry();
        var key = StrategyId.Create("A", "Instance");
        registry.TryAdd<string>(key, expectedStrategy).Should().BeTrue();

        registry.TryGet<string>(key, out var strategy).Should().BeTrue();

        strategy.Should().BeSameAs(expectedStrategy);
    }

    [Fact]
    public void TryAdd_Twice_SecondNotAdded()
    {
        var expectedStrategy = new TestResilienceStrategy();
        var registry = CreateRegistry();
        var key = StrategyId.Create("A", "Instance");
        registry.TryAdd(key, expectedStrategy).Should().BeTrue();

        registry.TryAdd(key, new TestResilienceStrategy()).Should().BeFalse();

        registry.TryGet(key, out var strategy).Should().BeTrue();
        strategy.Should().BeSameAs(expectedStrategy);
    }

    [Fact]
    public void TryAdd_GenericTwice_SecondNotAdded()
    {
        var expectedStrategy = new TestResilienceStrategy<string>();
        var registry = CreateRegistry();
        var key = StrategyId.Create("A", "Instance");
        registry.TryAdd(key, expectedStrategy).Should().BeTrue();

        registry.TryAdd(key, new TestResilienceStrategy<string>()).Should().BeFalse();

        registry.TryGet<string>(key, out var strategy).Should().BeTrue();
        strategy.Should().BeSameAs(expectedStrategy);
    }

    private ResilienceStrategyRegistry<StrategyId> CreateRegistry() => new(_options);
}
