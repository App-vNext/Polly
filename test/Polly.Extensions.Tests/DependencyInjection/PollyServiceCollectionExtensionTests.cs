using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Polly.Extensions.DependencyInjection;
using Polly.Extensions.Telemetry;
using Polly.Registry;
using Polly.Telemetry;

namespace Polly.Extensions.Tests.DependencyInjection;

public class PollyServiceCollectionExtensionTests
{
    private const string Key = "my-strategy";
    private ServiceCollection _services;

    public PollyServiceCollectionExtensionTests() => _services = new ServiceCollection();

    [Fact]
    public void AddResilienceStrategy_ArgValidation()
    {
        _services = null!;
        Assert.Throws<ArgumentNullException>(() => AddResilienceStrategy(Key));
        Assert.Throws<ArgumentNullException>(() => AddResilienceStrategy<string>(Key));

        _services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => _services.AddResilienceStrategy(
            Key,
            (Action<ResilienceStrategyBuilder, AddResilienceStrategyContext<string>>)null!));
        Assert.Throws<ArgumentNullException>(() => _services.AddResilienceStrategy(
            Key,
            (Action<ResilienceStrategyBuilder<string>, AddResilienceStrategyContext<string>>)null!));

        Assert.Throws<ArgumentNullException>(() => _services.AddResilienceStrategy(
            Key,
            (Action<ResilienceStrategyBuilder>)null!));

        Assert.Throws<ArgumentNullException>(() => _services.AddResilienceStrategy(
            Key,
            (Action<ResilienceStrategyBuilder<string>>)null!));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResilienceStrategy_EnsureRegisteredServices(bool generic)
    {
        if (generic)
        {
            AddResilienceStrategy<string>(Key);
        }
        else
        {
            AddResilienceStrategy(Key);
        }

        var serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetServices<ResilienceStrategyBuilder>().Should().NotBeNull();
        serviceProvider.GetServices<ResilienceStrategyRegistry<string>>().Should().NotBeNull();
        serviceProvider.GetServices<ResilienceStrategyProvider<string>>().Should().NotBeNull();
        serviceProvider.GetServices<ResilienceStrategyBuilder>().Should().NotBeSameAs(serviceProvider.GetServices<ResilienceStrategyBuilder>());
    }

    [Fact]
    public void AddResilienceStrategy_MultipleRegistries_Ok()
    {
        AddResilienceStrategy(Key);
        AddResilienceStrategy<string>(Key);
        AddResilienceStrategy<int>(Key);

        _services.AddResilienceStrategy(10, context => context.AddStrategy(new TestStrategy()));
        _services.AddResilienceStrategy<int, string>(10, context => context.AddStrategy(new TestStrategy()));
        _services.AddResilienceStrategy<int, int>(10, context => context.AddStrategy(new TestStrategy()));

        var serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetRequiredService<ResilienceStrategyRegistry<string>>().GetStrategy(Key).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResilienceStrategyRegistry<string>>().GetStrategy<string>(Key).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResilienceStrategyRegistry<string>>().GetStrategy<int>(Key).Should().NotBeNull();

        serviceProvider.GetRequiredService<ResilienceStrategyRegistry<int>>().GetStrategy(10).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResilienceStrategyRegistry<int>>().GetStrategy<string>(10).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResilienceStrategyRegistry<int>>().GetStrategy<int>(10).Should().NotBeNull();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResilienceStrategy_EnsureContextFilled(bool generic)
    {
        var asserted = false;

        if (generic)
        {
            _services.AddResilienceStrategy<string, string>(Key, (builder, context) =>
            {
                context.RegistryContext.Should().NotBeNull();
                context.StrategyKey.Should().Be(Key);
                context.BuilderName.Should().Be(Key);
                builder.Should().NotBeNull();
                context.ServiceProvider.Should().NotBeNull();
                builder.AddStrategy(new TestStrategy());
                asserted = true;
            });

            CreateProvider().GetStrategy<string>(Key);
        }
        else
        {
            _services.AddResilienceStrategy(Key, (builder, context) =>
            {
                context.StrategyKey.Should().Be(Key);
                builder.Should().NotBeNull();
                context.ServiceProvider.Should().NotBeNull();
                builder.AddStrategy(new TestStrategy());
                asserted = true;
            });

            CreateProvider().GetStrategy(Key);
        }

        asserted.Should().BeTrue();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResilienceStrategy_EnsureTelemetryEnabled(bool hasLogging)
    {
        ResilienceStrategyTelemetry? telemetry = null;

        if (hasLogging)
        {
            _services.AddLogging();
        }

        _services.AddResilienceStrategy(Key, builder =>
            builder.AddStrategy(context =>
            {
                telemetry = context.Telemetry;
                return new TestStrategy();
            },
            new TestResilienceStrategyOptions()));

        CreateProvider().GetStrategy(Key);

        var diagSource = telemetry!.GetType().GetProperty("DiagnosticSource", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(telemetry);
        diagSource.Should().BeOfType<ResilienceTelemetryDiagnosticSource>();

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
    public void AddResilienceStrategy_EnsureResilienceStrategyBuilderResolvedCorrectly()
    {
        var asserted = false;
        var key = new ResiliencePropertyKey<int>("A");

        AddResilienceStrategy(Key, context =>
        {
            context.BuilderProperties.TryGetValue(PollyDependencyInjectionKeys.ServiceProvider, out _).Should().BeTrue();
            asserted = true;
        });

        CreateProvider().GetStrategy(Key);

        asserted.Should().BeTrue();
    }

    [Fact]
    public void AddResilienceStrategy_EnsureServicesNotAddedTwice()
    {
        AddResilienceStrategy(Key);
        var count = _services.Count;

        AddResilienceStrategy(Key);

        _services.Count.Should().Be(count + 1);
    }

    [Fact]
    public void AddResilienceStrategy_Single_Ok()
    {
        AddResilienceStrategy(Key);

        var provider = CreateProvider();

        var strategy = provider.GetStrategy(Key);
        provider.GetStrategy("my-strategy").Should().BeSameAs(provider.GetStrategy("my-strategy"));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResilienceStrategy_Twice_LastOneWins(bool generic)
    {
        var firstCalled = false;
        var secondCalled = false;

        if (generic)
        {
            AddResilienceStrategy<string>(Key, _ => firstCalled = true);
            AddResilienceStrategy<string>(Key, _ => secondCalled = true);
            CreateProvider().GetStrategy<string>(Key);
        }
        else
        {
            AddResilienceStrategy(Key, _ => firstCalled = true);
            AddResilienceStrategy(Key, _ => secondCalled = true);
            CreateProvider().GetStrategy(Key);
        }

        firstCalled.Should().BeFalse();
        secondCalled.Should().BeTrue();
    }

    [Fact]
    public void AddResilienceStrategy_Multiple_Ok()
    {
        for (var i = 0; i < 10; i++)
        {
            AddResilienceStrategy(i.ToString(CultureInfo.InvariantCulture));
            AddResilienceStrategy<string>(i.ToString(CultureInfo.InvariantCulture));
            AddResilienceStrategy<int>(i.ToString(CultureInfo.InvariantCulture));
        }

        var provider = CreateProvider();

        Enumerable
            .Range(0, 10)
            .SelectMany(i =>
            {
                var name = i.ToString(CultureInfo.InvariantCulture);

                return new object[]
                {
                    provider.GetStrategy(name),
                    provider.GetStrategy<string>(name),
                    provider.GetStrategy<int>(name)
                };
            })
            .Distinct()
            .Should()
            .HaveCount(30);
    }

    [Fact]
    public void AddResilienceStrategyRegistry_Ok()
    {
        var provider = new ServiceCollection().AddResilienceStrategyRegistry<string>().BuildServiceProvider();

        provider.GetRequiredService<ResilienceStrategyRegistry<string>>().Should().NotBeNull();
        provider.GetRequiredService<ResilienceStrategyProvider<string>>().Should().NotBeNull();
        provider.GetRequiredService<ResilienceStrategyBuilder>().DiagnosticSource.Should().NotBeNull();
    }

    [Fact]
    public void AddResilienceStrategyRegistry_ConfigureCallback_Ok()
    {
        Func<string, string> formatter = s => s;

        var provider = new ServiceCollection().AddResilienceStrategyRegistry<string>(options => options.InstanceNameFormatter = formatter).BuildServiceProvider();

        provider.GetRequiredService<ResilienceStrategyRegistry<string>>().Should().NotBeNull();
        provider.GetRequiredService<ResilienceStrategyProvider<string>>().Should().NotBeNull();
        provider.GetRequiredService<IOptions<ResilienceStrategyRegistryOptions<string>>>().Value.InstanceNameFormatter.Should().Be(formatter);
    }

    private void AddResilienceStrategy(string key, Action<ResilienceStrategyBuilderContext>? onBuilding = null)
    {
        _services.AddResilienceStrategy(key, builder =>
        {
            builder.AddStrategy(context =>
            {
                onBuilding?.Invoke(context);
                return new TestStrategy();
            }, new TestResilienceStrategyOptions());
        });
    }

    private void AddResilienceStrategy<TResult>(string key, Action<ResilienceStrategyBuilderContext>? onBuilding = null)
    {
        _services.AddResilienceStrategy<string, TResult>(key, builder =>
        {
            builder.AddStrategy(context =>
            {
                onBuilding?.Invoke(context);
                return new TestStrategy();
            }, new TestResilienceStrategyOptions());
        });
    }

    private ResilienceStrategyProvider<string> CreateProvider()
    {
        return _services.BuildServiceProvider().GetRequiredService<ResilienceStrategyProvider<string>>();
    }

    private class TestStrategy : ResilienceStrategy
    {
        protected override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state) => new(Outcome.FromException<TResult>(new NotSupportedException()));
    }
}
