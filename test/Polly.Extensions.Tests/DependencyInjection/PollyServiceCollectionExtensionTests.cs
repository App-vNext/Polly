using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly.DependencyInjection;
using Polly.Registry;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.DependencyInjection;

public class PollyServiceCollectionExtensionTests
{
    private const string Key = "my-pipeline";
    private ServiceCollection _services;

    public PollyServiceCollectionExtensionTests() => _services = [];

    [Fact]
    public void AddResiliencePipeline_ArgValidation()
    {
        _services = null!;
        Assert.Throws<ArgumentNullException>(() => AddResiliencePipeline(Key));
        Assert.Throws<ArgumentNullException>(() => AddResiliencePipeline<string>(Key));

        _services = [];
        Assert.Throws<ArgumentNullException>(() => _services.AddResiliencePipeline(
            Key,
            (Action<ResiliencePipelineBuilder, AddResiliencePipelineContext<string>>)null!));
        Assert.Throws<ArgumentNullException>(() => _services.AddResiliencePipeline(
            Key,
            (Action<ResiliencePipelineBuilder<string>, AddResiliencePipelineContext<string>>)null!));

        Assert.Throws<ArgumentNullException>(() => _services.AddResiliencePipeline(
            Key,
            (Action<ResiliencePipelineBuilder>)null!));

        Assert.Throws<ArgumentNullException>(() => _services.AddResiliencePipeline(
            Key,
            (Action<ResiliencePipelineBuilder<string>>)null!));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResiliencePipeline_EnsureRegisteredServices(bool generic)
    {
        if (generic)
        {
            AddResiliencePipeline<string>(Key);
        }
        else
        {
            AddResiliencePipeline(Key);
        }

        var serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetServices<ResiliencePipelineBuilder>().Should().NotBeNull();
        serviceProvider.GetServices<ResiliencePipelineRegistry<string>>().Should().NotBeNull();
        serviceProvider.GetServices<ResiliencePipelineProvider<string>>().Should().NotBeNull();
        serviceProvider.GetServices<ResiliencePipelineBuilder>().Should().NotBeSameAs(serviceProvider.GetServices<ResiliencePipelineBuilder>());
    }

    [Fact]
    public void AddResiliencePipeline_MultipleRegistries_Ok()
    {
        AddResiliencePipeline(Key);
        AddResiliencePipeline<string>(Key);
        AddResiliencePipeline<int>(Key);

        _services.AddResiliencePipeline(10, context => context.AddStrategy(new TestStrategy()));
        _services.AddResiliencePipeline<int, string>(10, context => context.AddStrategy(new TestStrategy()));
        _services.AddResiliencePipeline<int, int>(10, context => context.AddStrategy(new TestStrategy()));

        var serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<string>>().GetPipeline(Key).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<string>>().GetPipeline<string>(Key).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<string>>().GetPipeline<int>(Key).Should().NotBeNull();

        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<int>>().GetPipeline(10).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<int>>().GetPipeline<string>(10).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<int>>().GetPipeline<int>(10).Should().NotBeNull();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResiliencePipeline_EnsureContextFilled(bool generic)
    {
        var asserted = false;

        if (generic)
        {
            _services.AddResiliencePipeline<string, string>(Key, (builder, context) =>
            {
                context.RegistryContext.Should().NotBeNull();
                context.PipelineKey.Should().Be(Key);
                builder.Name.Should().Be(Key);
                builder.Should().NotBeNull();
                context.ServiceProvider.Should().NotBeNull();
                builder.AddStrategy(new TestStrategy());
                asserted = true;
            });

            CreateProvider().GetPipeline<string>(Key);
        }
        else
        {
            _services.AddResiliencePipeline(Key, (builder, context) =>
            {
                context.PipelineKey.Should().Be(Key);
                builder.Should().NotBeNull();
                context.ServiceProvider.Should().NotBeNull();
                builder.AddStrategy(new TestStrategy());
                asserted = true;
            });

            CreateProvider().GetPipeline(Key);
        }

        asserted.Should().BeTrue();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResiliencePipeline_EnsureTelemetryEnabled(bool hasLogging)
    {
        ResilienceStrategyTelemetry? telemetry = null;

        if (hasLogging)
        {
            _services.AddLogging();
        }

        _services.AddResiliencePipeline(Key, builder =>
            builder.AddStrategy(context =>
            {
                telemetry = context.Telemetry;
                return new TestStrategy();
            },
            new TestResilienceStrategyOptions()));

        CreateProvider().GetPipeline(Key);

        var diagSource = telemetry!.GetType().GetProperty("Listener", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(telemetry);
        diagSource.Should().BeOfType<TelemetryListenerImpl>();

        var factory = _services.BuildServiceProvider().GetRequiredService<IOptions<TelemetryOptions>>().Value.LoggerFactory;

        if (hasLogging)
        {
            factory.Should().NotBe(NullLoggerFactory.Instance);
        }
        else
        {
            factory.Should().Be(NullLoggerFactory.Instance);
        }
    }

    [Fact]
    public void AddResiliencePipeline_EnsureResiliencePipelineBuilderResolvedCorrectly()
    {
        var asserted = false;
        var key = new ResiliencePropertyKey<int>("A");

        AddResiliencePipeline(Key, context => asserted = true);

        CreateProvider().GetPipeline(Key);

        asserted.Should().BeTrue();
    }

    [Fact]
    public void AddResiliencePipeline_EnsureServicesNotAddedTwice()
    {
        AddResiliencePipeline(Key);
        var count = _services.Count;

        AddResiliencePipeline(Key);

        _services.Count.Should().Be(count + 1);
    }

    [Fact]
    public void AddResiliencePipeline_Single_Ok()
    {
        AddResiliencePipeline(Key);

        var provider = CreateProvider();

        var pipeline = provider.GetPipeline(Key);
        provider.GetPipeline("my-pipeline").Should().BeSameAs(provider.GetPipeline("my-pipeline"));
    }

    [Fact]
    public void AddResiliencePipeline_KeyedSingleton_Ok()
    {
        AddResiliencePipeline(Key);

        var provider = _services.BuildServiceProvider();

        var pipeline = provider.GetKeyedService<ResiliencePipeline>(Key);
        provider.GetKeyedService<ResiliencePipeline>(Key).Should().BeSameAs(pipeline);

        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddResiliencePipeline_GenericKeyedSingleton_Ok()
    {
        AddResiliencePipeline<string>(Key);

        var provider = _services.BuildServiceProvider();

        var pipeline = provider.GetKeyedService<ResiliencePipeline<string>>(Key);
        provider.GetKeyedService<ResiliencePipeline<string>>(Key).Should().BeSameAs(pipeline);

        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void AddResiliencePipeline_KeyedSingletonOverride_Ok()
    {
        var pipeline = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromSeconds(1)).Build();
        _services.AddKeyedSingleton(Key, pipeline);
        AddResiliencePipeline(Key);

        var provider = _services.BuildServiceProvider();

        provider.GetKeyedService<ResiliencePipeline>(Key).Should().BeSameAs(pipeline);
    }

    [Fact]
    public void AddResiliencePipeline_GenericKeyedSingletonOverride_Ok()
    {
        var pipeline = new ResiliencePipelineBuilder<string>().AddTimeout(TimeSpan.FromSeconds(1)).Build();
        _services.AddKeyedSingleton(Key, pipeline);
        AddResiliencePipeline(Key);

        var provider = _services.BuildServiceProvider();

        provider.GetKeyedService<ResiliencePipeline<string>>(Key).Should().BeSameAs(pipeline);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResiliencePipeline_Twice_FirstOneWins(bool generic)
    {
        var firstCalled = false;
        var secondCalled = false;

        if (generic)
        {
            AddResiliencePipeline<string>(Key, _ => firstCalled = true);
            AddResiliencePipeline<string>(Key, _ => secondCalled = true);
            CreateProvider().GetPipeline<string>(Key);
        }
        else
        {
            AddResiliencePipeline(Key, _ => firstCalled = true);
            AddResiliencePipeline(Key, _ => secondCalled = true);
            CreateProvider().GetPipeline(Key);
        }

        firstCalled.Should().BeTrue();
        secondCalled.Should().BeFalse();
    }

    [Fact]
    public void AddResiliencePipeline_Multiple_Ok()
    {
        for (var i = 0; i < 10; i++)
        {
            AddResiliencePipeline(i.ToString(CultureInfo.InvariantCulture));
            AddResiliencePipeline<string>(i.ToString(CultureInfo.InvariantCulture));
            AddResiliencePipeline<int>(i.ToString(CultureInfo.InvariantCulture));
        }

        var provider = CreateProvider();

        Enumerable
            .Range(0, 10)
            .SelectMany(i =>
            {
                var name = i.ToString(CultureInfo.InvariantCulture);

                return new object[]
                {
                    provider.GetPipeline(name),
                    provider.GetPipeline<string>(name),
                    provider.GetPipeline<int>(name)
                };
            })
            .Distinct()
            .Should()
            .HaveCount(30);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResiliencePipeline_EnsureTimeProvider(bool timeProviderRegistered)
    {
        var timeProvider = Substitute.For<TimeProvider>();
        var asserted = false;

        if (timeProviderRegistered)
        {
            _services.TryAddSingleton(timeProvider);
        }

        _services.AddResiliencePipeline("dummy", builder =>
        {
            if (timeProviderRegistered)
            {
                builder.TimeProvider.Should().Be(timeProvider);
            }
            else
            {
                builder.TimeProvider.Should().BeNull();
            }

            asserted = true;
        });

        CreateProvider().GetPipeline("dummy");
        asserted.Should().BeTrue();
    }

    [Fact]
    public void AddResiliencePipelineRegistry_Ok()
    {
        var provider = new ServiceCollection().AddResiliencePipelineRegistry<string>().BuildServiceProvider();

        provider.GetRequiredService<ResiliencePipelineRegistry<string>>().Should().NotBeNull();
        provider.GetRequiredService<ResiliencePipelineProvider<string>>().Should().NotBeNull();
        provider.GetRequiredService<ResiliencePipelineBuilder>().TelemetryListener.Should().NotBeNull();
    }

    [Fact]
    public void AddResiliencePipelineRegistry_ConfigureCallback_Ok()
    {
        Func<string, string> formatter = s => s;

        var provider = new ServiceCollection().AddResiliencePipelineRegistry<string>(options => options.InstanceNameFormatter = formatter).BuildServiceProvider();

        provider.GetRequiredService<ResiliencePipelineRegistry<string>>().Should().NotBeNull();
        provider.GetRequiredService<ResiliencePipelineProvider<string>>().Should().NotBeNull();
        provider.GetRequiredService<IOptions<ResiliencePipelineRegistryOptions<string>>>().Value.InstanceNameFormatter.Should().Be(formatter);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResiliencePipeline_CustomInstanceName_EnsureReported(bool usingBuilder)
    {
        // arrange
        using var loggerFactory = new FakeLoggerFactory();

        var context = ResilienceContextPool.Shared.Get("my-operation_key");
        var services = new ServiceCollection();
        var listener = new FakeTelemetryListener();
        var registry = services
            .AddResiliencePipeline("my-pipeline", ConfigureBuilder)
            .Configure<TelemetryOptions>(options => options.TelemetryListeners.Add(listener))
            .AddSingleton((ILoggerFactory)loggerFactory)
            .BuildServiceProvider()
            .GetRequiredService<ResiliencePipelineRegistry<string>>();

        var pipeline = usingBuilder ?
            registry.GetPipeline("my-pipeline") :
            registry.GetOrAddPipeline("my-pipeline", ConfigureBuilder);

        // act
        pipeline.Execute(_ => { }, context);

        // assert
        foreach (var ev in listener.Events)
        {
            ev.Source.PipelineInstanceName.Should().Be("my-instance");
            ev.Source.PipelineName.Should().Be("my-pipeline");
        }

        var record = loggerFactory.FakeLogger.GetRecords(new EventId(0, "ResilienceEvent")).First();

        record.Message.Should().Contain("my-pipeline/my-instance");

        static void ConfigureBuilder(ResiliencePipelineBuilder builder)
        {
            builder.Name.Should().Be("my-pipeline");
            builder.InstanceName = "my-instance";
            builder.AddRetry(new()
            {
                ShouldHandle = _ => PredicateResult.True(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.Zero
            });
        }
    }

    [Fact]
    public void AddResiliencePipelines_Multiple_Ok()
    {
        _services.AddResiliencePipelines<string>(ctx =>
        {
            for (var i = 0; i < 10; i++)
            {
                ctx.AddResiliencePipeline(i.ToString(CultureInfo.InvariantCulture),
                    (builder, _) => builder.AddStrategy(new TestStrategy()));
                ctx.AddResiliencePipeline<string>(i.ToString(CultureInfo.InvariantCulture),
                    (builder, _) => builder.AddStrategy(new TestStrategy()));
                ctx.AddResiliencePipeline<int>(i.ToString(CultureInfo.InvariantCulture),
                    (builder, _) => builder.AddStrategy(new TestStrategy()));
            }
        });

        var provider = CreateProvider();

        Enumerable
            .Range(0, 10)
            .SelectMany(i =>
            {
                var name = i.ToString(CultureInfo.InvariantCulture);

                return new object[]
                {
                    provider.GetPipeline(name),
                    provider.GetPipeline<string>(name),
                    provider.GetPipeline<int>(name)
                };
            })
            .Distinct()
            .Should()
            .HaveCount(30);
    }

    [Fact]
    public void AddResiliencePipelines_MultipleRegistries_Ok()
    {
        _services.AddResiliencePipelines<string>(ctx =>
        {
            ctx.AddResiliencePipeline(Key, (builder, _) => builder.AddStrategy(new TestStrategy()));
            ctx.AddResiliencePipeline<string>(Key, (builder, _) => builder.AddStrategy(new TestStrategy()));
            ctx.AddResiliencePipeline<int>(Key, (builder, _) => builder.AddStrategy(new TestStrategy()));
        });

        _services.AddResiliencePipelines<int>(ctx =>
        {
            ctx.AddResiliencePipeline(10, (builder, _) => builder.AddStrategy(new TestStrategy()));
            ctx.AddResiliencePipeline<string>(10, (builder, _) => builder.AddStrategy(new TestStrategy()));
            ctx.AddResiliencePipeline<int>(10, (builder, _) => builder.AddStrategy(new TestStrategy()));
        });

        var serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<string>>().GetPipeline(Key).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<string>>().GetPipeline<string>(Key).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<string>>().GetPipeline<int>(Key).Should().NotBeNull();

        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<int>>().GetPipeline(10).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<int>>().GetPipeline<string>(10).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResiliencePipelineRegistry<int>>().GetPipeline<int>(10).Should().NotBeNull();
    }

    private void AddResiliencePipeline(string key, Action<StrategyBuilderContext>? onBuilding = null)
    {
        _services.AddResiliencePipeline(key, builder =>
        {
            builder.AddStrategy(context =>
            {
                onBuilding?.Invoke(context);
                return new TestStrategy();
            }, new TestResilienceStrategyOptions());
        });
    }

    private void AddResiliencePipeline<TResult>(string key, Action<StrategyBuilderContext>? onBuilding = null)
    {
        _services.AddResiliencePipeline<string, TResult>(key, builder =>
        {
            builder.AddStrategy(context =>
            {
                onBuilding?.Invoke(context);
                return new TestStrategy();
            }, new TestResilienceStrategyOptions());
        });
    }

    private ResiliencePipelineProvider<string> CreateProvider()
    {
        return _services.BuildServiceProvider().GetRequiredService<ResiliencePipelineProvider<string>>();
    }

    private class TestStrategy : ResilienceStrategy
    {
        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state) => new(Outcome.FromException<TResult>(new NotSupportedException()));
    }
}
