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
        descriptor.IsReloadable.ShouldBeFalse();
        descriptor.Strategies.Count.ShouldBe(7);
        descriptor.FirstStrategy.Options.ShouldBeOfType<FallbackStrategyOptions<string>>();
        descriptor.Strategies[0].Options.ShouldBeOfType<FallbackStrategyOptions<string>>();
        descriptor.Strategies[0].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("Fallback");
        descriptor.Strategies[1].Options.ShouldBeOfType<RetryStrategyOptions<string>>();
        descriptor.Strategies[1].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("Retry");
        descriptor.Strategies[2].Options.ShouldBeOfType<CircuitBreakerStrategyOptions<string>>();
        descriptor.Strategies[2].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("CircuitBreaker");
        descriptor.Strategies[3].Options.ShouldBeOfType<TimeoutStrategyOptions>();
        descriptor.Strategies[3].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("Timeout");
        descriptor.Strategies[3].Options
            .ShouldBeOfType<TimeoutStrategyOptions>().Timeout
            .ShouldBe(TimeSpan.FromSeconds(1));

        descriptor.Strategies[4].Options.ShouldBeOfType<HedgingStrategyOptions<string>>();
        descriptor.Strategies[4].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("Hedging");
        descriptor.Strategies[5].Options.ShouldBeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[5].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("RateLimiter");
        descriptor.Strategies[6].StrategyInstance.ShouldBeOfType<CustomStrategy>();
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
        descriptor.IsReloadable.ShouldBeFalse();
        descriptor.Strategies.Count.ShouldBe(5);
        descriptor.Strategies[0].Options.ShouldBeOfType<RetryStrategyOptions>();
        descriptor.Strategies[0].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("Retry");
        descriptor.Strategies[1].Options.ShouldBeOfType<CircuitBreakerStrategyOptions>();
        descriptor.Strategies[1].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("CircuitBreaker");
        descriptor.Strategies[2].Options.ShouldBeOfType<TimeoutStrategyOptions>();
        descriptor.Strategies[2].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("Timeout");
        descriptor.Strategies[2].Options
            .ShouldBeOfType<TimeoutStrategyOptions>().Timeout
            .ShouldBe(TimeSpan.FromSeconds(1));

        descriptor.Strategies[3].Options.ShouldBeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[3].StrategyInstance.GetType().FullName.ShouldNotBeNull().ShouldContain("RateLimiter");
        descriptor.Strategies[4].StrategyInstance.ShouldBeOfType<CustomStrategy>();
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
        descriptor.IsReloadable.ShouldBeFalse();
        descriptor.Strategies.Count.ShouldBe(1);
        descriptor.Strategies[0].Options.ShouldBeOfType<TimeoutStrategyOptions>();
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
        descriptor.IsReloadable.ShouldBeTrue();
        descriptor.Strategies.Count.ShouldBe(2);
        descriptor.Strategies[0].Options.ShouldBeOfType<RateLimiterStrategyOptions>();
        descriptor.Strategies[1].StrategyInstance.ShouldBeOfType<CustomStrategy>();
    }

    [Fact]
    public void GetPipelineDescriptor_InnerPipeline_Ok()
    {
        var descriptor = new ResiliencePipelineBuilder()
            .AddPipeline(new ResiliencePipelineBuilder().AddConcurrencyLimiter(1).Build())
            .Build()
            .GetPipelineDescriptor();

        descriptor.Strategies.Count.ShouldBe(1);
        descriptor.Strategies[0].Options.ShouldBeOfType<RateLimiterStrategyOptions>();
    }

    private sealed class CustomStrategy : ResilienceStrategy
    {
        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => throw new NotSupportedException();
    }

    private class TestOptions : ResilienceStrategyOptions;
}
