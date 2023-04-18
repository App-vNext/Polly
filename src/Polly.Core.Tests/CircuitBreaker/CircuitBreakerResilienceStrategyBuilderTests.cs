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
}
