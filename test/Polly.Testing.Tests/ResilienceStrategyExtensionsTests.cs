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
            .AddStrategy(_ => new CustomStrategy(), new TestOptions())
            .ConfigureTelemetry(NullLoggerFactory.Instance)
            .Build();

        // act
        var descriptor = strategy.GetInnerStrategies();

        // assert
        descriptor.IsReloadable.Should().BeFalse();
        descriptor.Strategies.Should().HaveCount(7);
        descriptor.FirstStrategy.Options.Should().BeOfType<FallbackStrategyOptions<string>>();
        descriptor.Strategies[0].Options.Should().BeOfType<FallbackStrategyOptions<string>>();
        descriptor.Strategies[0].StrategyInstance.GetType().FullName.Should().Contain("Fallback");
        descriptor.Strategies[1].Options.Should().BeOfType<RetryStrategyOptions<string>>();
        descriptor.Strategies[1].StrategyInstance.GetType().FullName.Should().Contain("Retry");
        descriptor.Strategies[2].Options.Should().BeOfType<CircuitBreakerStrategyOptions<string>>();
        descriptor.Strategies[2].StrategyInstance.GetType().FullName.Should().Contain("CircuitBreaker");
        descriptor.Strategies[3].Options.Should().BeOfType<TimeoutStrategyOptions>();
        descriptor.Strategies[3].StrategyInstance.GetType().FullName.Should().Contain("Timeout");
        descriptor.Strategies[3].Options
            .Should()
            .BeOfType<TimeoutStrategyOptions>().Subject.Timeout
            .Should().Be(TimeSpan.FromSeconds(1));

        descriptor.Strategies[4].Options.Should().BeOfType<HedgingStrategyOptions<string>>();
        descriptor.Strategies[4].StrategyInstance.GetType().FullName.Should().Contain("Hedging");
        descriptor.Strategies[5].Options.Should().BeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[5].StrategyInstance.GetType().FullName.Should().Contain("RateLimiter");
        descriptor.Strategies[6].StrategyInstance.GetType().Should().Be(typeof(CustomStrategy));
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
            .AddStrategy(_ => new CustomStrategy(), new TestOptions())
            .ConfigureTelemetry(NullLoggerFactory.Instance)
            .Build();

        // act
        var descriptor = strategy.GetInnerStrategies();

        // assert
        descriptor.IsReloadable.Should().BeFalse();
        descriptor.Strategies.Should().HaveCount(5);
        descriptor.Strategies[0].Options.Should().BeOfType<RetryStrategyOptions>();
        descriptor.Strategies[0].StrategyInstance.GetType().FullName.Should().Contain("Retry");
        descriptor.Strategies[1].Options.Should().BeOfType<CircuitBreakerStrategyOptions>();
        descriptor.Strategies[1].StrategyInstance.GetType().FullName.Should().Contain("CircuitBreaker");
        descriptor.Strategies[2].Options.Should().BeOfType<TimeoutStrategyOptions>();
        descriptor.Strategies[2].StrategyInstance.GetType().FullName.Should().Contain("Timeout");
        descriptor.Strategies[2].Options
            .Should()
            .BeOfType<TimeoutStrategyOptions>().Subject.Timeout
            .Should().Be(TimeSpan.FromSeconds(1));

        descriptor.Strategies[3].Options.Should().BeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[3].StrategyInstance.GetType().FullName.Should().Contain("RateLimiter");
        descriptor.Strategies[4].StrategyInstance.GetType().Should().Be(typeof(CustomStrategy));
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
                .AddStrategy(_ => new CustomStrategy(), new TestOptions());
        });

        // act
        var descriptor = strategy.GetInnerStrategies();

        // assert
        descriptor.IsReloadable.Should().BeTrue();
        descriptor.Strategies.Should().HaveCount(2);
        descriptor.Strategies[0].Options.Should().BeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[1].StrategyInstance.GetType().Should().Be(typeof(CustomStrategy));
    }

    private sealed class CustomStrategy : NonReactiveResilienceStrategy
    {
        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => throw new NotSupportedException();
    }

    private class TestOptions : ResilienceStrategyOptions
    {
    }
}
