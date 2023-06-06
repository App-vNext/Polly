using System.ComponentModel.DataAnnotations;
using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerResilienceStrategyBuilderTests
{
    public static TheoryData<Action<ResilienceStrategyBuilder>> ConfigureData = new()
    {
        builder => builder.AddAdvancedCircuitBreaker(new AdvancedCircuitBreakerStrategyOptions
        {
            ShouldHandle = _ => PredicateResult.True
        }),
        builder => builder.AddSimpleCircuitBreaker(new SimpleCircuitBreakerStrategyOptions
        {
            ShouldHandle = _ => PredicateResult.True
        }),
    };

    public static TheoryData<Action<ResilienceStrategyBuilder<int>>> ConfigureDataGeneric = new()
    {
        builder => builder.AddAdvancedCircuitBreaker(new AdvancedCircuitBreakerStrategyOptions<int>
        {
            ShouldHandle = _ => PredicateResult.True
        }),
        builder => builder.AddSimpleCircuitBreaker(new SimpleCircuitBreakerStrategyOptions<int>
        {
            ShouldHandle = _ => PredicateResult.True
        }),
    };

    [MemberData(nameof(ConfigureData))]
    [Theory]
    public void AddCircuitBreaker_Configure(Action<ResilienceStrategyBuilder> builderAction)
    {
        var builder = new ResilienceStrategyBuilder();

        builderAction(builder);

        var strategy = builder.Build();

        strategy.Should().BeOfType<CircuitBreakerResilienceStrategy>();
    }

    [MemberData(nameof(ConfigureDataGeneric))]
    [Theory]
    public void AddCircuitBreaker_Generic_Configure(Action<ResilienceStrategyBuilder<int>> builderAction)
    {
        var builder = new ResilienceStrategyBuilder<int>();

        builderAction(builder);

        var strategy = builder.Build().Strategy;

        strategy.Should().BeOfType<CircuitBreakerResilienceStrategy>();
    }

    [Fact]
    public void AddCircuitBreaker_Validation()
    {
        new ResilienceStrategyBuilder<int>()
            .Invoking(b => b.AddSimpleCircuitBreaker(new SimpleCircuitBreakerStrategyOptions<int> { BreakDuration = TimeSpan.MinValue }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The circuit breaker strategy options are invalid.*");

        new ResilienceStrategyBuilder()
            .Invoking(b => b.AddSimpleCircuitBreaker(new SimpleCircuitBreakerStrategyOptions { BreakDuration = TimeSpan.MinValue }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The circuit breaker strategy options are invalid.*");
    }

    [Fact]
    public void AddAdvancedCircuitBreaker_Validation()
    {
        new ResilienceStrategyBuilder<int>()
            .Invoking(b => b.AddAdvancedCircuitBreaker(new AdvancedCircuitBreakerStrategyOptions<int> { BreakDuration = TimeSpan.MinValue }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The advanced circuit breaker strategy options are invalid.*");

        new ResilienceStrategyBuilder()
            .Invoking(b => b.AddAdvancedCircuitBreaker(new AdvancedCircuitBreakerStrategyOptions { BreakDuration = TimeSpan.MinValue }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The advanced circuit breaker strategy options are invalid.*");
    }

    [Fact]
    public void AddCircuitBreaker_IntegrationTest()
    {
        int opened = 0;
        int closed = 0;
        int halfOpened = 0;

        var options = new SimpleCircuitBreakerStrategyOptions
        {
            FailureThreshold = 5,
            BreakDuration = TimeSpan.FromMilliseconds(500),
            ShouldHandle = args => new ValueTask<bool>(args.Result is -1),
            OnOpened = _ => { opened++; return default; },
            OnClosed = _ => { closed++; return default; },
            OnHalfOpened = (_) => { halfOpened++; return default; }
        };

        var timeProvider = new FakeTimeProvider();
        var strategy = new ResilienceStrategyBuilder { TimeProvider = timeProvider.Object }.AddSimpleCircuitBreaker(options).Build();
        var time = DateTime.UtcNow;
        timeProvider.Setup(v => v.UtcNow).Returns(() => time);

        for (int i = 0; i < options.FailureThreshold; i++)
        {
            strategy.Execute(_ => -1);
        }

        // Circuit opened
        opened.Should().Be(1);
        halfOpened.Should().Be(0);
        closed.Should().Be(0);
        Assert.Throws<BrokenCircuitException<int>>(() => strategy.Execute(_ => 0));

        // Circuit Half Opened
        time += options.BreakDuration;
        strategy.Execute(_ => -1);
        Assert.Throws<BrokenCircuitException<int>>(() => strategy.Execute(_ => 0));
        opened.Should().Be(2);
        halfOpened.Should().Be(1);
        closed.Should().Be(0);

        // Now close it
        time += options.BreakDuration;
        strategy.Execute(_ => 0);
        opened.Should().Be(2);
        halfOpened.Should().Be(2);
        closed.Should().Be(1);
    }

    [Fact]
    public void AddAdvancedCircuitBreaker_IntegrationTest()
    {
        int opened = 0;
        int closed = 0;
        int halfOpened = 0;

        var options = new AdvancedCircuitBreakerStrategyOptions
        {
            FailureThreshold = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromSeconds(10),
            BreakDuration = TimeSpan.FromSeconds(1),
            ShouldHandle = args => new ValueTask<bool>(args.Result is -1),
            OnOpened = _ => { opened++; return default; },
            OnClosed = _ => { closed++; return default; },
            OnHalfOpened = (_) => { halfOpened++; return default; }
        };

        var timeProvider = new FakeTimeProvider();
        var strategy = new ResilienceStrategyBuilder { TimeProvider = timeProvider.Object }.AddAdvancedCircuitBreaker(options).Build();
        var time = DateTime.UtcNow;
        timeProvider.Setup(v => v.UtcNow).Returns(() => time);

        for (int i = 0; i < 10; i++)
        {
            strategy.Execute(_ => -1);
        }

        // Circuit opened
        opened.Should().Be(1);
        halfOpened.Should().Be(0);
        closed.Should().Be(0);
        Assert.Throws<BrokenCircuitException<int>>(() => strategy.Execute(_ => 0));

        // Circuit Half Opened
        time += options.BreakDuration;
        strategy.Execute(_ => -1);
        Assert.Throws<BrokenCircuitException<int>>(() => strategy.Execute(_ => 0));
        opened.Should().Be(2);
        halfOpened.Should().Be(1);
        closed.Should().Be(0);

        // Now close it
        time += options.BreakDuration;
        strategy.Execute(_ => 0);
        opened.Should().Be(2);
        halfOpened.Should().Be(2);
        closed.Should().Be(1);
    }
}
