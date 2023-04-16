using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.DependencyInjection;
using Polly.Registry;
using Polly.Strategy;

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
        var asserted = false;

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
    public void AddResilienceStrategy_EnsureResilienceStrategyBuilderResolvedCorrectly()
    {
        var asserted = false;
        var key = new ResiliencePropertyKey<int>("A");

        AddResilienceStrategy(Key, context =>
        {
            context.BuilderProperties.TryGetValue(PollyDependencyInjectionKeys.ServiceProvider, out _).Should().BeTrue();
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
        var firstCalled = false;
        var secondCalled = false;

        AddResilienceStrategy(Key, _ => firstCalled = true);
        AddResilienceStrategy(Key, _ => secondCalled = true);

        CreateProvider().Get(Key);

        firstCalled.Should().BeFalse();
        secondCalled.Should().BeTrue();
    }

    [Fact]
    public void AddResilienceStrategy_Multiple_Ok()
    {
        for (var i = 0; i < 10; i++)
        {
            AddResilienceStrategy(i.ToString(CultureInfo.InvariantCulture));
        }

        var provider = CreateProvider();

        Enumerable.Range(0, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)).Distinct().Should().HaveCount(10);
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
