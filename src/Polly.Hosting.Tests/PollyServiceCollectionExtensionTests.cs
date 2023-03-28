using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Polly.Builder;
using Polly.DependencyInjection;
using Polly.Registry;
using Polly.Telemetry;

namespace Polly.Hosting.Tests;
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

        _services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => _services.AddResilienceStrategy(Key, null!));
    }

    [Fact]
    public void AddResilienceStrategy_EnsureRegisteredServices()
    {
        AddResilienceStrategy(Key);

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
        _services.AddResilienceStrategy(10, context => context.Builder.AddStrategy(new TestStrategy()));

        var serviceProvider = _services.BuildServiceProvider();

        serviceProvider.GetRequiredService<ResilienceStrategyRegistry<string>>().Get(Key).Should().NotBeNull();
        serviceProvider.GetRequiredService<ResilienceStrategyRegistry<int>>().Get(10).Should().NotBeNull();
    }

    [Fact]
    public void AddResilienceStrategy_EnsureContextFilled()
    {
        bool asserted = false;

        _services.AddResilienceStrategy(Key, context =>
        {
            context.Key.Should().Be(Key);
            context.Builder.Should().NotBeNull();
            context.ServiceProvider.Should().NotBeNull();
            context.Builder.AddStrategy(new TestStrategy());
            asserted = true;
        });

        CreateProvider().Get(Key);

        asserted.Should().BeTrue();
    }

    [Fact]
    public void AddResilienceStrategy_EnsureResilienceStrategyBuilderOptionsApplied()
    {
        var telemetry = Mock.Of<ResilienceTelemetry>();
        var telemetryFactory = Mock.Of<ResilienceTelemetryFactory>(v => v.Create(It.IsAny<ResilienceTelemetryFactoryContext>()) == telemetry);
        var asserted = false;
        var key = new ResiliencePropertyKey<int>("A");
        ResilienceStrategyBuilderOptions? globalOptions = null;

        _services.Configure<ResilienceStrategyBuilderOptions>(options =>
        {
            options.BuilderName = "dummy";
            options.TelemetryFactory = telemetryFactory;
            options.Properties.Set(key, 123);
            globalOptions = options;
        });

        AddResilienceStrategy(Key, context =>
        {
            context.BuilderProperties.Should().NotBeSameAs(globalOptions!.Properties);
            context.BuilderName.Should().Be("dummy");
            context.Telemetry.Should().Be(telemetry);
            context.BuilderProperties.TryGetValue(key, out var val).Should().BeTrue();
            val.Should().Be(123);
            asserted = true;
        });

        CreateProvider().Get(Key);

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

        var strategy = provider.Get(Key);
        strategy.Should().BeOfType<TestStrategy>();
        provider.Get("my-strategy").Should().BeSameAs(provider.Get("my-strategy"));
    }

    [Fact]
    public void AddResilienceStrategy_Twice_LastOneWins()
    {
        bool firstCalled = false;
        bool secondCalled = false;

        AddResilienceStrategy(Key, _ => firstCalled = true);
        AddResilienceStrategy(Key, _ => secondCalled = true);

        CreateProvider().Get(Key);

        firstCalled.Should().BeFalse();
        secondCalled.Should().BeTrue();
    }

    [Fact]
    public void AddResilienceStrategy_Multiple_Ok()
    {
        for (int i = 0; i < 10; i++)
        {
            AddResilienceStrategy(i.ToString(CultureInfo.InvariantCulture));
        }

        var provider = CreateProvider();

        Enumerable.Range(0, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)).Distinct().Should().HaveCount(10);
    }

    [Fact]
    public void AddResilienceStrategy_CustomTelemetryFactory_EnsureUsed()
    {
        var telemetry = new Mock<ResilienceTelemetry>(MockBehavior.Strict);
        var factory = new Mock<ResilienceTelemetryFactory>(MockBehavior.Strict);
        factory.Setup(v => v.Create(It.IsAny<ResilienceTelemetryFactoryContext>())).Returns(telemetry.Object);

        var asserted = false;

        _services.AddSingleton<ResilienceTelemetryFactory>(factory.Object);
        _services.AddResilienceStrategy(
            Key,
            context =>
            {
                context.Builder.Options.TelemetryFactory.Should().Be(factory.Object);
                context.Builder.AddStrategy(context =>
                {
                    context.Telemetry.Should().Be(telemetry.Object);

                    asserted = true;
                    return new TestStrategy();
                });
            });

        CreateProvider().Get(Key);

        asserted.Should().BeTrue();
    }

    private void AddResilienceStrategy(string key, Action<ResilienceStrategyBuilderContext>? onBuilding = null)
    {
        _services.AddResilienceStrategy(key, context =>
        {
            context.Builder.AddStrategy(context =>
            {
                onBuilding?.Invoke(context);
                return new TestStrategy();
            });
        });
    }

    private ResilienceStrategyProvider<string> CreateProvider()
    {
        return _services.BuildServiceProvider().GetRequiredService<ResilienceStrategyProvider<string>>();
    }

    private class TestStrategy : ResilienceStrategy
    {
        protected override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<TResult>> callback,
            ResilienceContext context,
            TState state) => throw new NotSupportedException();
    }
}
