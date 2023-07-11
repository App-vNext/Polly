using System;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Hedging;
using Polly.RateLimiting;
using Polly.Registry;
using Polly.Retry;
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
        strategies.HasTelemetry.Should().BeTrue();
        strategies.Strategies.Should().HaveCount(7);
        strategies.Strategies[0].Options.Should().BeOfType<FallbackStrategyOptions<string>>();
        strategies.Strategies[1].Options.Should().BeOfType<RetryStrategyOptions<string>>();
        strategies.Strategies[2].Options.Should().BeOfType<AdvancedCircuitBreakerStrategyOptions<string>>();
        strategies.Strategies[3].Options.Should().BeOfType<TimeoutStrategyOptions>();
        strategies.Strategies[3].Options
            .Should()
            .BeOfType<TimeoutStrategyOptions>().Subject.Timeout
            .Should().Be(TimeSpan.FromSeconds(1));

        strategies.Strategies[4].Options.Should().BeOfType<HedgingStrategyOptions<string>>();
        strategies.Strategies[5].Options.Should().BeOfType<RateLimiterStrategyOptions>();
        strategies.Strategies[6].StrategyType.Should().Be(typeof(CustomStrategy));
    }

    [Fact]
    public void GetInnerStrategies_Reloadable_Ok()
    {
        // arrange
        var strategy = new ResilienceStrategyRegistry<string>().GetOrAddStrategy("dummy", (builder, context) =>
        {
            context.EnableReloads(() => () => CancellationToken.None);

            builder
                .AddConcurrencyLimiter(10)
                .AddStrategy(new CustomStrategy());
        });

        // act
        var strategies = strategy.GetInnerStrategies();

        // assert
        strategies.IsReloadable.Should().BeTrue();
        strategies.HasTelemetry.Should().BeFalse();
        strategies.Strategies.Should().HaveCount(2);
        strategies.Strategies[0].Options.Should().BeOfType<RateLimiterStrategyOptions>();
        strategies.Strategies[1].StrategyType.Should().Be(typeof(CustomStrategy));
    }

    private sealed class CustomStrategy : ResilienceStrategy
    {
        protected override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => throw new NotSupportedException();
    }
}
