using System.Globalization;
using Polly.Registry;
using Polly.Retry;
using Polly.Testing;
using Polly.Timeout;
using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests.Registry;

public class ResiliencePipelineRegistryTests
{
    private readonly ResiliencePipelineRegistryOptions<StrategyId> _options;

    private Action<ResiliencePipelineBuilder> _callback = _ => { };

    public ResiliencePipelineRegistryTests() => _options = new()
    {
        BuilderFactory = () =>
        {
            var builder = new ResiliencePipelineBuilder();
            _callback(builder);
            return builder;
        },
        PipelineComparer = StrategyId.Comparer,
        BuilderComparer = StrategyId.BuilderComparer
    };

    [Fact]
    public void Ctor_Default_Ok() =>
        this.Invoking(_ => new ResiliencePipelineRegistry<string>()).Should().NotThrow();

    [Fact]
    public void Ctor_InvalidOptions_Throws() =>
        this.Invoking(_ => new ResiliencePipelineRegistry<string>(new ResiliencePipelineRegistryOptions<string> { BuilderFactory = null! }))
            .Should()
            .Throw<ArgumentNullException>();

    [Fact]
    public void GetPipeline_BuilderMultiInstance_EnsureMultipleInstances()
    {
        var builderName = "A";
        using var registry = CreateRegistry();
        var strategies = new HashSet<ResiliencePipeline>();
        registry.TryAddBuilder(StrategyId.Create(builderName), (builder, _) => builder.AddStrategy(new TestResilienceStrategy()));

        for (int i = 0; i < 100; i++)
        {
            var key = StrategyId.Create(builderName, i.ToString(CultureInfo.InvariantCulture));

            strategies.Add(registry.GetPipeline(key));

            // call again, the strategy should be already cached
            strategies.Add(registry.GetPipeline(key));
        }

        strategies.Should().HaveCount(100);
    }

    [Fact]
    public void GetPipeline_GenericBuilderMultiInstance_EnsureMultipleInstances()
    {
        var builderName = "A";
        using var registry = CreateRegistry();
        var strategies = new HashSet<ResiliencePipeline<string>>();
        registry.TryAddBuilder<string>(StrategyId.Create(builderName), (builder, _) => builder.AddStrategy(new TestResilienceStrategy()));

        for (int i = 0; i < 100; i++)
        {
            var key = StrategyId.Create(builderName, i.ToString(CultureInfo.InvariantCulture));

            strategies.Add(registry.GetPipeline<string>(key));

            // call again, the strategy should be already cached
            strategies.Add(registry.GetPipeline<string>(key));
        }

        strategies.Should().HaveCount(100);
    }

    [Fact]
    public void TryAddBuilder_GetPipeline_EnsureCalled()
    {
        var activatorCalls = 0;
        _callback = _ => activatorCalls++;
        using var registry = CreateRegistry();
        var called = 0;
        registry.TryAddBuilder(StrategyId.Create("A"), (builder, _) =>
        {
            builder.AddStrategy(new TestResilienceStrategy());
            called++;
        });

        var key1 = StrategyId.Create("A");
        var key2 = StrategyId.Create("A", "Instance1");
        var key3 = StrategyId.Create("A", "Instance2");
        var keys = new[] { key1, key2, key3 };
        var strategies = keys.ToDictionary(k => k, registry.GetPipeline);
        foreach (var key in keys)
        {
            registry.GetPipeline(key);
        }

        called.Should().Be(3);
        activatorCalls.Should().Be(3);
        strategies.Keys.Should().HaveCount(3);
    }

    [Fact]
    public void TryAddBuilder_GenericGetPipeline_EnsureCalled()
    {
        var activatorCalls = 0;
        _callback = _ => activatorCalls++;
        using var registry = CreateRegistry();
        var called = 0;
        registry.TryAddBuilder<string>(StrategyId.Create("A"), (builder, _) =>
        {
            builder.AddStrategy(new TestResilienceStrategy());
            called++;
        });

        var key1 = StrategyId.Create("A");
        var key2 = StrategyId.Create("A", "Instance1");
        var key3 = StrategyId.Create("A", "Instance2");
        var keys = new[] { key1, key2, key3 };
        var strategies = keys.ToDictionary(k => k, registry.GetPipeline<string>);
        foreach (var key in keys)
        {
            registry.GetPipeline<string>(key);
        }

        called.Should().Be(3);
        activatorCalls.Should().Be(3);
        strategies.Keys.Should().HaveCount(3);
    }

    [Fact]
    public void TryAddBuilder_EnsurePipelineKey()
    {
        _options.BuilderNameFormatter = k => k.BuilderName;
        _options.InstanceNameFormatter = k => k.InstanceName;

        var called = false;
        using var registry = CreateRegistry();
        registry.TryAddBuilder(StrategyId.Create("A"), (builder, context) =>
        {
            context.BuilderName.Should().Be("A");
            context.BuilderInstanceName.Should().Be("Instance1");
            context.PipelineKey.Should().Be(StrategyId.Create("A", "Instance1"));

            builder.AddStrategy(new TestResilienceStrategy());
            builder.Name.Should().Be("A");
            called = true;
        });

        registry.GetPipeline(StrategyId.Create("A", "Instance1"));
        called.Should().BeTrue();
    }

    [InlineData(false)]
    [InlineData(true)]
    [Theory]
    public void TryAddBuilder_Twice_EnsureCorrectBehavior(bool generic)
    {
        using var registry = new ResiliencePipelineRegistry<string>();

        var called1 = false;
        var called2 = false;

        AddBuilder(() => called1 = true).Should().BeTrue();
        AddBuilder(() => called2 = true).Should().BeFalse();

        if (generic)
        {
            registry.GetPipeline<string>("A");
        }
        else
        {
            registry.GetPipeline("A");
        }

        called1.Should().BeTrue();
        called2.Should().BeFalse();

        bool AddBuilder(Action onCalled)
        {
            if (generic)
            {
                return registry!.TryAddBuilder<string>("A", (_, _) => onCalled());
            }
            else
            {
                return registry!.TryAddBuilder("A", (_, _) => onCalled());
            }
        }
    }

    [Fact]
    public void TryAddBuilder_MultipleGeneric_EnsureDistinctInstances()
    {
        using var registry = CreateRegistry();
        registry.TryAddBuilder<string>(StrategyId.Create("A"), (builder, _) => builder.AddStrategy(new TestResilienceStrategy()));
        registry.TryAddBuilder<int>(StrategyId.Create("A"), (builder, _) => builder.AddStrategy(new TestResilienceStrategy()));

        registry.GetPipeline<string>(StrategyId.Create("A", "Instance1")).Should().BeSameAs(registry.GetPipeline<string>(StrategyId.Create("A", "Instance1")));
        registry.GetPipeline<int>(StrategyId.Create("A", "Instance1")).Should().BeSameAs(registry.GetPipeline<int>(StrategyId.Create("A", "Instance1")));
    }

    [Fact]
    public void TryAddBuilder_Generic_EnsurePipelineKey()
    {
        _options.BuilderNameFormatter = k => k.BuilderName;
        _options.InstanceNameFormatter = k => k.InstanceName;

        var called = false;
        using var registry = CreateRegistry();
        registry.TryAddBuilder<string>(StrategyId.Create("A"), (builder, _) =>
        {
            builder.AddStrategy(new TestResilienceStrategy());
            builder.Name.Should().Be("A");
            builder.InstanceName.Should().Be("Instance1");
            called = true;
        });

        registry.GetPipeline<string>(StrategyId.Create("A", "Instance1"));
        called.Should().BeTrue();
    }

    [Fact]
    public void TryGet_NoBuilder_Null()
    {
        using var registry = CreateRegistry();
        var key = StrategyId.Create("A");

        registry.TryGetPipeline(key, out var strategy).Should().BeFalse();
        strategy.Should().BeNull();
    }

    [Fact]
    public void TryGet_GenericNoBuilder_Null()
    {
        using var registry = CreateRegistry();
        var key = StrategyId.Create("A");

        registry.TryGetPipeline<string>(key, out var strategy).Should().BeFalse();
        strategy.Should().BeNull();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void EnableReloads_Ok(bool firstOne)
    {
        // arrange
        var retryCount = 2;
        using var registry = new ResiliencePipelineRegistry<string>();
        using var token1 = new CancellationTokenSource();
        using var token2 = new CancellationTokenSource();

        registry.TryAddBuilder("dummy", (builder, context) =>
        {
            // this call enables dynamic reloads for the dummy strategy
            context.AddReloadToken(token1.Token);
            context.AddReloadToken(token2.Token);

            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = _ => PredicateResult.True(),
                MaxRetryAttempts = retryCount,
                Delay = TimeSpan.FromMilliseconds(2),
            });
        });

        // act
        var strategy = registry.GetPipeline("dummy");

        // assert
        var tries = 0;
        strategy.Execute(() => tries++);
        tries.Should().Be(retryCount + 1);

        tries = 0;
        retryCount = 5;

        if (firstOne)
        {
            token1.Cancel();
        }
        else
        {
            token2.Cancel();
        }

        strategy.Execute(() => tries++);
        tries.Should().Be(retryCount + 1);
    }

    [Fact]
    public void EnableReloads_EnsureDisposedCallbackCalled()
    {
        // arrange
        var registry = new ResiliencePipelineRegistry<string>();
        using var changeSource = new CancellationTokenSource();
        var disposedCalls = 0;

        registry.TryAddBuilder("dummy", (builder, context) =>
        {
            // this call enables dynamic reloads for the dummy strategy
            context.AddReloadToken(changeSource.Token);
            context.OnPipelineDisposed(() => disposedCalls++);
            builder.AddTimeout(TimeSpan.FromSeconds(1));
        });

        // act
        var strategy = registry.GetPipeline("dummy");

        // assert
        disposedCalls.Should().Be(0);
        strategy.Execute(() => { });

        changeSource.Cancel();
        disposedCalls.Should().Be(1);
        strategy.Execute(() => { });

        registry.Dispose();
        disposedCalls.Should().Be(2);
    }

    [Fact]
    public void EnableReloads_Generic_Ok()
    {
        // arrange
        var retryCount = 2;
        using var registry = new ResiliencePipelineRegistry<string>();
        using var changeSource = new CancellationTokenSource();

        registry.TryAddBuilder<string>("dummy", (builder, context) =>
        {
            // this call enables dynamic reloads for the dummy strategy
            context.AddReloadToken(changeSource.Token);

            builder.AddRetry(new RetryStrategyOptions<string>
            {
                ShouldHandle = _ => PredicateResult.True(),
                MaxRetryAttempts = retryCount,
                Delay = TimeSpan.FromMilliseconds(2),
            });
        });

        // act
        var strategy = registry.GetPipeline<string>("dummy");

        // assert
        var tries = 0;
        strategy.Execute(() => { tries++; return "dummy"; });
        tries.Should().Be(retryCount + 1);

        tries = 0;
        retryCount = 5;
        changeSource.Cancel();
        strategy.Execute(() => { tries++; return "dummy"; });
        tries.Should().Be(retryCount + 1);
    }

    [Fact]
    public void GetOrAddPipeline_Ok()
    {
        var id = new StrategyId(typeof(string), "A");
        var called = 0;

        using var registry = CreateRegistry();
        var strategy = registry.GetOrAddPipeline(id, builder => { builder.AddTimeout(TimeSpan.FromSeconds(1)); called++; });
        var otherPipeline = registry.GetOrAddPipeline(id, builder => { builder.AddTimeout(TimeSpan.FromSeconds(1)); called++; });

        strategy.GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<TimeoutResilienceStrategy>();

        called.Should().Be(1);
    }

    [Fact]
    public void GetOrAddPipeline_EnsureCorrectComponents()
    {
        var id = new StrategyId(typeof(string), "A");

        using var registry = CreateRegistry();

        var pipeline = registry.GetOrAddPipeline(id, builder => builder.AddTimeout(TimeSpan.FromSeconds(1)));
        pipeline.Component.Should().BeOfType<ExecutionTrackingComponent>().Subject.Component.Should().BeOfType<CompositeComponent>();

        var genericPipeline = registry.GetOrAddPipeline<string>(id, builder => builder.AddTimeout(TimeSpan.FromSeconds(1)));
        pipeline.Component.Should().BeOfType<ExecutionTrackingComponent>().Subject.Component.Should().BeOfType<CompositeComponent>();
    }

    [Fact]
    public void GetOrAddPipeline_Generic_Ok()
    {
        var id = new StrategyId(typeof(string), "A");
        var called = 0;

        using var registry = CreateRegistry();
        var strategy = registry.GetOrAddPipeline<string>(id, builder => { builder.AddTimeout(TimeSpan.FromSeconds(1)); called++; });
        var otherPipeline = registry.GetOrAddPipeline<string>(id, builder => { builder.AddTimeout(TimeSpan.FromSeconds(1)); called++; });

        strategy.GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<TimeoutResilienceStrategy>();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Dispose_EnsureDisposed(bool isAsync)
    {
        var registry = CreateRegistry();

        var pipeline1 = registry.GetOrAddPipeline(StrategyId.Create("A"), builder => { builder.AddTimeout(TimeSpan.FromSeconds(1)); });
        var pipeline2 = registry.GetOrAddPipeline(StrategyId.Create("B"), builder => { builder.AddTimeout(TimeSpan.FromSeconds(1)); });
        var pipeline3 = registry.GetOrAddPipeline<string>(StrategyId.Create("C"), builder => { builder.AddTimeout(TimeSpan.FromSeconds(1)); });
        var pipeline4 = registry.GetOrAddPipeline<string>(StrategyId.Create("D"), builder => { builder.AddTimeout(TimeSpan.FromSeconds(1)); });

#pragma warning disable S3966 // Objects should not be disposed more than once
        if (isAsync)
        {
            await registry.DisposeAsync();
            await registry.DisposeAsync();
        }
        else
        {
            registry.Dispose();
            registry.Dispose();
        }
#pragma warning restore S3966 // Objects should not be disposed more than once

        pipeline1.Invoking(p => p.Execute(() => { })).Should().Throw<ObjectDisposedException>();
        pipeline2.Invoking(p => p.Execute(() => { })).Should().Throw<ObjectDisposedException>();
        pipeline3.Invoking(p => p.Execute(() => "dummy")).Should().Throw<ObjectDisposedException>();
        pipeline4.Invoking(p => p.Execute(() => "dummy")).Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task DisposePipeline_NotAllowed()
    {
        using var registry = CreateRegistry();
        var pipeline = registry.GetOrAddPipeline(StrategyId.Create("A"), builder => { builder.AddTimeout(TimeSpan.FromSeconds(1)); });

        await pipeline.Invoking(p => p.DisposeHelper.DisposeAsync().AsTask()).Should().ThrowAsync<InvalidOperationException>();

    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Dispose_EnsureNotUsableAnymore(bool isAsync)
    {
        using var registry = new ResiliencePipelineRegistry<string>();
        await DisposeHelper.TryDisposeSafeAsync(registry, !isAsync);

        registry.Invoking(r => r.GetOrAddPipeline("dummy", builder => { })).Should().Throw<ObjectDisposedException>();
        registry.Invoking(r => r.GetOrAddPipeline<string>("dummy", builder => { })).Should().Throw<ObjectDisposedException>();
        registry.Invoking(r => r.GetOrAddPipeline("dummy", (_, _) => { })).Should().Throw<ObjectDisposedException>();
        registry.Invoking(r => r.GetOrAddPipeline<string>("dummy", (_, _) => { })).Should().Throw<ObjectDisposedException>();
        registry.Invoking(r => r.TryAddBuilder("dummy", (_, _) => { })).Should().Throw<ObjectDisposedException>();
        registry.Invoking(r => r.TryAddBuilder<string>("dummy", (_, _) => { })).Should().Throw<ObjectDisposedException>();
        registry.Invoking(r => r.GetPipeline<string>("dummy")).Should().Throw<ObjectDisposedException>();
        registry.Invoking(r => r.GetPipeline("dummy")).Should().Throw<ObjectDisposedException>();
        registry.Invoking(r => r.TryGetPipeline<string>("dummy", out _)).Should().Throw<ObjectDisposedException>();
        registry.Invoking(r => r.TryGetPipeline("dummy", out _)).Should().Throw<ObjectDisposedException>();
    }

    private ResiliencePipelineRegistry<StrategyId> CreateRegistry() => new(_options);
}
