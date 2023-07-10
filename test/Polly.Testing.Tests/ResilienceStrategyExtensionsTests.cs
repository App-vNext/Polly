using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Timeout;

namespace Polly.Testing.Tests;

public class ResilienceStrategyExtensionsTests
{
    [Fact]
    public void GetInnerStrategies_Ok()
    {
        // arrange
        var strategy = new ResilienceStrategyBuilder<string>()
            .AddFallback(new()
            {
                FallbackAction = _ => Outcome.FromResultAsTask("dummy"),
            })
            .AddRetry(new())
            .AddAdvancedCircuitBreaker(new())
            .AddTimeout(TimeSpan.FromSeconds(1))
            .AddHedging(new())
            .AddConcurrencyLimiter(10)
            .AddStrategy(new CustomStrategy())
            .ConfigureTelemetry(NullLoggerFactory.Instance)
            .Build();

        // act
        var strategies = strategy.GetInnerStrategies();

        // assert
        strategies.Should().HaveCount(8);
        strategies[0].Type.Should().Be(ResilienceStrategyType.Telemetry);
        strategies[1].Type.Should().Be(ResilienceStrategyType.Fallback);
        strategies[2].Type.Should().Be(ResilienceStrategyType.Retry);
        strategies[3].Type.Should().Be(ResilienceStrategyType.CircuitBreaker);
        strategies[4].Type.Should().Be(ResilienceStrategyType.Timeout);
        strategies[4].Options
            .Should()
            .BeOfType<TimeoutStrategyOptions>().Subject.Timeout
            .Should().Be(TimeSpan.FromSeconds(1));

        strategies[5].Type.Should().Be(ResilienceStrategyType.Hedging);
        strategies[6].Type.Should().Be(ResilienceStrategyType.RateLimiter);
        strategies[7].Type.Should().Be(ResilienceStrategyType.Custom);
        strategies[7].StrategyType.Should().Be(typeof(CustomStrategy));
    }

    private sealed class CustomStrategy : ResilienceStrategy
    {
        protected override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => throw new NotSupportedException();
    }
}
