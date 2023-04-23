using System.ComponentModel.DataAnnotations;
using Polly.CircuitBreaker;
using Xunit;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerResilienceStrategyBuilderTests
{
    public static TheoryData<Action<ResilienceStrategyBuilder>> ConfigureData = new()
    {
        builder =>
        {
            builder.AddAdvancedCircuitBreaker(new AdvancedCircuitBreakerStrategyOptions<int>());
        },
         builder =>
        {
            builder.AddAdvancedCircuitBreaker(new AdvancedCircuitBreakerStrategyOptions());
        },
        builder =>
        {
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<int>());
        },
        builder =>
        {
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions());
        },
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

    [Fact]
    public void AddCircuitBreaker_Validation()
    {
        var builder = new ResilienceStrategyBuilder();

        builder
            .Invoking(b => b.AddCircuitBreaker(new CircuitBreakerStrategyOptions<int> { BreakDuration = TimeSpan.MinValue }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The circuit breaker strategy options are invalid.*");

        builder
            .Invoking(b => b.AddCircuitBreaker(new CircuitBreakerStrategyOptions { BreakDuration = TimeSpan.MinValue }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The circuit breaker strategy options are invalid.*");
    }

    [Fact]
    public void AddAdvancedCircuitBreaker_Validation()
    {
        var builder = new ResilienceStrategyBuilder();

        builder
            .Invoking(b => b.AddAdvancedCircuitBreaker(new AdvancedCircuitBreakerStrategyOptions<int> { BreakDuration = TimeSpan.MinValue }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The advanced circuit breaker strategy options are invalid.*");

        builder
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

        var options = new CircuitBreakerStrategyOptions
        {
            FailureThreshold = 5,
            BreakDuration = TimeSpan.FromMilliseconds(500),
        };

        options.ShouldHandle.HandleResult(-1);
        options.OnOpened.Register<int>(() => opened++);
        options.OnClosed.Register<int>(() => closed++);
        options.OnHalfOpened.Register(() => halfOpened++);

        var timeProvider = new FakeTimeProvider();
        var strategy = new ResilienceStrategyBuilder { TimeProvider = timeProvider.Object }.AddCircuitBreaker(options).Build();
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
        var options = new AdvancedCircuitBreakerStrategyOptions
        {
            BreakDuration = TimeSpan.FromMilliseconds(500),
        };

        options.ShouldHandle.HandleResult(-1);
        options.OnOpened.Register<int>(() => { });
        options.OnClosed.Register<int>(() => { });
        options.OnHalfOpened.Register(() => { });

        var timeProvider = new FakeTimeProvider();
        var strategy = new ResilienceStrategyBuilder { TimeProvider = timeProvider.Object }.AddAdvancedCircuitBreaker(options).Build();
        var time = DateTime.UtcNow;
        timeProvider.Setup(v => v.UtcNow).Returns(() => time);

        strategy.Should().BeOfType<CircuitBreakerResilienceStrategy>();
    }

    [Fact]
    public void AddCircuitBreaker_UnrecognizedOptions_Throws()
    {
        var builder = new ResilienceStrategyBuilder();

        builder.Invoking(b => b.AddCircuitBreakerCore(new DummyOptions()).Build()).Should().Throw<NotSupportedException>();
    }

    private class DummyOptions : BaseCircuitBreakerStrategyOptions
    {
    }
}
