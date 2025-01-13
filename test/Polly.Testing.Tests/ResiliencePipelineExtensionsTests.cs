using Microsoft.Extensions.Logging.Abstractions;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Hedging;
using Polly.RateLimiting;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;

namespace Polly.Testing.Tests;

public class ResiliencePipelineExtensionsTests
{
    [Fact]
    public void GetPipelineDescriptor_Generic_Ok()
    {
        // arrange
        var strategy = new ResiliencePipelineBuilder<string>()
            .AddFallback(new()
            {
                FallbackAction = _ => Outcome.FromResultAsValueTask("dummy"),
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
        var descriptor = strategy.GetPipelineDescriptor();

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
        descriptor.Strategies[6].StrategyInstance.GetType().Should().Be<CustomStrategy>();
    }

    [Fact]
    public void GetPipelineDescriptor_NonGeneric_Ok()
    {
        // arrange
        var strategy = new ResiliencePipelineBuilder()
            .AddRetry(new())
            .AddCircuitBreaker(new())
            .AddTimeout(TimeSpan.FromSeconds(1))
            .AddConcurrencyLimiter(10)
            .AddStrategy(_ => new CustomStrategy(), new TestOptions())
            .ConfigureTelemetry(NullLoggerFactory.Instance)
            .Build();

        // act
        var descriptor = strategy.GetPipelineDescriptor();

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
        descriptor.Strategies[4].StrategyInstance.GetType().Should().Be<CustomStrategy>();
    }

    [Fact]
    public void GetPipelineDescriptor_SingleStrategy_Ok()
    {
        // arrange
        var strategy = new ResiliencePipelineBuilder<string>()
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        // act
        var descriptor = strategy.GetPipelineDescriptor();

        // assert
        descriptor.IsReloadable.Should().BeFalse();
        descriptor.Strategies.Should().HaveCount(1);
        descriptor.Strategies[0].Options.Should().BeOfType<TimeoutStrategyOptions>();
    }

    [Fact]
    public async Task GetPipelineDescriptor_Reloadable_Ok()
    {
        // arrange
        using var source = new CancellationTokenSource();
        await using var registry = new ResiliencePipelineRegistry<string>();
        var strategy = registry.GetOrAddPipeline("dummy", (builder, context) =>
        {
            context.OnPipelineDisposed(() => { });
            context.AddReloadToken(source.Token);

            builder
                .AddConcurrencyLimiter(10)
                .AddStrategy(_ => new CustomStrategy(), new TestOptions());
        });

        // act
        var descriptor = strategy.GetPipelineDescriptor();

        // assert
        descriptor.IsReloadable.Should().BeTrue();
        descriptor.Strategies.Should().HaveCount(2);
        descriptor.Strategies[0].Options.Should().BeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[1].StrategyInstance.GetType().Should().Be<CustomStrategy>();
    }

    [Fact]
    public void GetPipelineDescriptor_InnerPipeline_Ok()
    {
        var descriptor = new ResiliencePipelineBuilder()
            .AddPipeline(new ResiliencePipelineBuilder().AddConcurrencyLimiter(1).Build())
            .Build()
            .GetPipelineDescriptor();

        descriptor.Strategies.Should().HaveCount(1);
        descriptor.Strategies[0].Options.Should().BeOfType<RateLimiterStrategyOptions>();
    }

    private sealed class CustomStrategy : ResilienceStrategy
    {
        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => throw new NotSupportedException();
    }

    private class TestOptions : ResilienceStrategyOptions
    {
    }
}
