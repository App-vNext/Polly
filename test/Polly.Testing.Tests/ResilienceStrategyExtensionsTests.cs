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
    public void GetInnerStrategies_Generic_Ok()
    {
        // arrange
        var strategy = new CompositeStrategyBuilder<string>()
            .AddFallback(new()
            {
                FallbackAction = _ => Outcome.FromResultAsTask("dummy"),
            })
            .AddRetry(new())
            .AddCircuitBreaker(new())
            .AddTimeout(TimeSpan.FromSeconds(1))
            .AddHedging(new())
            .AddConcurrencyLimiter(10)
            .AddStrategy(new CustomStrategy())
            .ConfigureTelemetry(NullLoggerFactory.Instance)
            .Build();

        // act
        var descriptor = strategy.GetInnerStrategies();

        // assert
        descriptor.HasTelemetry.Should().BeTrue();
        descriptor.IsReloadable.Should().BeFalse();
        descriptor.Strategies.Should().HaveCount(7);
        descriptor.Strategies[0].Options.Should().BeOfType<FallbackStrategyOptions<string>>();
        descriptor.Strategies[0].StrategyType.FullName.Should().Contain("Fallback");
        descriptor.Strategies[1].Options.Should().BeOfType<RetryStrategyOptions<string>>();
        descriptor.Strategies[1].StrategyType.FullName.Should().Contain("Retry");
        descriptor.Strategies[2].Options.Should().BeOfType<CircuitBreakerStrategyOptions<string>>();
        descriptor.Strategies[2].StrategyType.FullName.Should().Contain("CircuitBreaker");
        descriptor.Strategies[3].Options.Should().BeOfType<TimeoutStrategyOptions>();
        descriptor.Strategies[3].StrategyType.FullName.Should().Contain("Timeout");
        descriptor.Strategies[3].Options
            .Should()
            .BeOfType<TimeoutStrategyOptions>().Subject.Timeout
            .Should().Be(TimeSpan.FromSeconds(1));

        descriptor.Strategies[4].Options.Should().BeOfType<HedgingStrategyOptions<string>>();
        descriptor.Strategies[4].StrategyType.FullName.Should().Contain("Hedging");
        descriptor.Strategies[5].Options.Should().BeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[5].StrategyType.FullName.Should().Contain("RateLimiter");
        descriptor.Strategies[6].StrategyType.Should().Be(typeof(CustomStrategy));
    }

    [Fact]
    public void GetInnerStrategies_NonGeneric_Ok()
    {
        // arrange
        var strategy = new CompositeStrategyBuilder()
            .AddRetry(new())
            .AddCircuitBreaker(new())
            .AddTimeout(TimeSpan.FromSeconds(1))
            .AddConcurrencyLimiter(10)
            .AddStrategy(new CustomStrategy())
            .ConfigureTelemetry(NullLoggerFactory.Instance)
            .Build();

        // act
        var descriptor = strategy.GetInnerStrategies();

        // assert
        descriptor.HasTelemetry.Should().BeTrue();
        descriptor.IsReloadable.Should().BeFalse();
        descriptor.Strategies.Should().HaveCount(5);
        descriptor.Strategies[0].Options.Should().BeOfType<RetryStrategyOptions>();
        descriptor.Strategies[0].StrategyType.FullName.Should().Contain("Retry");
        descriptor.Strategies[1].Options.Should().BeOfType<CircuitBreakerStrategyOptions>();
        descriptor.Strategies[1].StrategyType.FullName.Should().Contain("CircuitBreaker");
        descriptor.Strategies[2].Options.Should().BeOfType<TimeoutStrategyOptions>();
        descriptor.Strategies[2].StrategyType.FullName.Should().Contain("Timeout");
        descriptor.Strategies[2].Options
            .Should()
            .BeOfType<TimeoutStrategyOptions>().Subject.Timeout
            .Should().Be(TimeSpan.FromSeconds(1));

        descriptor.Strategies[3].Options.Should().BeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[3].StrategyType.FullName.Should().Contain("RateLimiter");
        descriptor.Strategies[4].StrategyType.Should().Be(typeof(CustomStrategy));
    }

    [Fact]
    public void GetInnerStrategies_SingleStrategy_Ok()
    {
        // arrange
        var strategy = new CompositeStrategyBuilder<string>()
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        // act
        var descriptor = strategy.GetInnerStrategies();

        // assert
        descriptor.HasTelemetry.Should().BeFalse();
        descriptor.IsReloadable.Should().BeFalse();
        descriptor.Strategies.Should().HaveCount(1);
        descriptor.Strategies[0].Options.Should().BeOfType<TimeoutStrategyOptions>();
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
        var descriptor = strategy.GetInnerStrategies();

        // assert
        descriptor.HasTelemetry.Should().BeFalse();
        descriptor.IsReloadable.Should().BeTrue();
        descriptor.Strategies.Should().HaveCount(2);
        descriptor.Strategies[0].Options.Should().BeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[1].StrategyType.Should().Be(typeof(CustomStrategy));
    }

    private sealed class CustomStrategy : ResilienceStrategy
    {
        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => throw new NotSupportedException();
    }
}
