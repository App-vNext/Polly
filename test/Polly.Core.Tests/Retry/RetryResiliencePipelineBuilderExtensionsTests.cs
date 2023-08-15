using System.ComponentModel.DataAnnotations;
using Polly.Retry;
using Polly.Testing;

namespace Polly.Core.Tests.Retry;

#pragma warning disable CA2012 // Use ValueTasks correctly

public class RetryResiliencePipelineBuilderExtensionsTests
{
    public static readonly TheoryData<Action<ResiliencePipelineBuilder>> OverloadsData = new()
    {
        builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                BackoffType = RetryBackoffType.Exponential,
                RetryCount = 3,
                BaseDelay = TimeSpan.FromSeconds(2),
                ShouldHandle = _ => PredicateResult.True,
            });

            AssertStrategy(builder, RetryBackoffType.Exponential, 3, TimeSpan.FromSeconds(2));
        }
    };

    public static readonly TheoryData<Action<ResiliencePipelineBuilder<int>>> OverloadsDataGeneric = new()
    {
        builder =>
        {
            builder.AddRetry(new RetryStrategyOptions<int>
            {
                BackoffType = RetryBackoffType.Exponential,
                RetryCount = 3,
                BaseDelay = TimeSpan.FromSeconds(2),
                ShouldHandle = _ => PredicateResult.True
            });

            AssertStrategy(builder, RetryBackoffType.Exponential, 3, TimeSpan.FromSeconds(2));
        }
    };

    [MemberData(nameof(OverloadsData))]
    [Theory]
    public void AddRetry_Overloads_Ok(Action<ResiliencePipelineBuilder> configure)
    {
        var builder = new ResiliencePipelineBuilder();

        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [MemberData(nameof(OverloadsDataGeneric))]
    [Theory]
    public void AddRetry_GenericOverloads_Ok(Action<ResiliencePipelineBuilder<int>> configure)
    {
        var builder = new ResiliencePipelineBuilder<int>();

        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [Fact]
    public void AddRetry_DefaultOptions_Ok()
    {
        var builder = new ResiliencePipelineBuilder();
        var options = new RetryStrategyOptions { ShouldHandle = _ => PredicateResult.True };

        builder.AddRetry(options);

        AssertStrategy(builder, options.BackoffType, options.RetryCount, options.BaseDelay);
    }

    private static void AssertStrategy(ResiliencePipelineBuilder builder, RetryBackoffType type, int retries, TimeSpan delay, Action<RetryResilienceStrategy<object>>? assert = null)
    {
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<RetryResilienceStrategy<object>>().Subject;

        strategy.BackoffType.Should().Be(type);
        strategy.RetryCount.Should().Be(retries);
        strategy.BaseDelay.Should().Be(delay);

        assert?.Invoke(strategy);
    }

    private static void AssertStrategy<T>(
        ResiliencePipelineBuilder<T> builder,
        RetryBackoffType type,
        int retries,
        TimeSpan delay,
        Action<RetryResilienceStrategy<T>>? assert = null)
    {
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<RetryResilienceStrategy<T>>().Subject;

        strategy.BackoffType.Should().Be(type);
        strategy.RetryCount.Should().Be(retries);
        strategy.BaseDelay.Should().Be(delay);

        assert?.Invoke(strategy);
    }

    [Fact]
    public void AddRetry_InvalidOptions_Throws()
    {
        new ResiliencePipelineBuilder()
            .Invoking(b => b.AddRetry(new RetryStrategyOptions { ShouldHandle = null! }))
            .Should()
            .Throw<ValidationException>();

        new ResiliencePipelineBuilder<int>()
            .Invoking(b => b.AddRetry(new RetryStrategyOptions<int> { ShouldHandle = null! }))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void GetAggregatedDelay_ShouldReturnTheSameValue()
    {
        var options = new RetryStrategyOptions { BackoffType = RetryBackoffType.Exponential, UseJitter = true };

        var delay = GetAggregatedDelay(options);
        GetAggregatedDelay(options).Should().Be(delay);
    }

    [Fact]
    public void GetAggregatedDelay_EnsureCorrectValue()
    {
        var options = new RetryStrategyOptions { BackoffType = RetryBackoffType.Constant, BaseDelay = TimeSpan.FromSeconds(1), RetryCount = 5 };

        GetAggregatedDelay(options).Should().Be(TimeSpan.FromSeconds(5));
    }

    private static TimeSpan GetAggregatedDelay<T>(RetryStrategyOptions<T> options)
    {
        var aggregatedDelay = TimeSpan.Zero;

        var strategy = new ResiliencePipelineBuilder().AddRetry(new()
        {
            RetryCount = options.RetryCount,
            BaseDelay = options.BaseDelay,
            BackoffType = options.BackoffType,
            ShouldHandle = _ => PredicateResult.True, // always retry until all retries are exhausted
            RetryDelayGenerator = args =>
            {
                // the delay hint is calculated for this attempt by the retry strategy
                aggregatedDelay += args.Arguments.DelayHint;

                // return zero delay, so no waiting
                return new ValueTask<TimeSpan>(TimeSpan.Zero);
            },
            Randomizer = () => 1.0,
        })
        .Build();

        // this executes all retries and we aggregate the delays immediately
        strategy.Execute(() => { });

        return aggregatedDelay;
    }
}
