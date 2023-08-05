using System.ComponentModel.DataAnnotations;
using Polly.Retry;

namespace Polly.Core.Tests.Retry;

#pragma warning disable CA2012 // Use ValueTasks correctly

public class RetryCompositeStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<CompositeStrategyBuilder>> OverloadsData = new()
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

    public static readonly TheoryData<Action<CompositeStrategyBuilder<int>>> OverloadsDataGeneric = new()
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
    public void AddRetry_Overloads_Ok(Action<CompositeStrategyBuilder> configure)
    {
        var builder = new CompositeStrategyBuilder();

        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [MemberData(nameof(OverloadsDataGeneric))]
    [Theory]
    public void AddRetry_GenericOverloads_Ok(Action<CompositeStrategyBuilder<int>> configure)
    {
        var builder = new CompositeStrategyBuilder<int>();

        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [Fact]
    public void AddRetry_DefaultOptions_Ok()
    {
        var builder = new CompositeStrategyBuilder();
        var options = new RetryStrategyOptions { ShouldHandle = _ => PredicateResult.True };

        builder.AddRetry(options);

        AssertStrategy(builder, options.BackoffType, options.RetryCount, options.BaseDelay);
    }

    private static void AssertStrategy(CompositeStrategyBuilder builder, RetryBackoffType type, int retries, TimeSpan delay, Action<RetryResilienceStrategy<object>>? assert = null)
    {
        var strategy = (RetryResilienceStrategy<object>)builder.Build();

        strategy.BackoffType.Should().Be(type);
        strategy.RetryCount.Should().Be(retries);
        strategy.BaseDelay.Should().Be(delay);

        assert?.Invoke(strategy);
    }

    private static void AssertStrategy<T>(CompositeStrategyBuilder<T> builder, RetryBackoffType type, int retries, TimeSpan delay, Action<RetryResilienceStrategy<T>>? assert = null)
    {
        var strategy = (RetryResilienceStrategy<T>)builder.Build().Strategy;

        strategy.BackoffType.Should().Be(type);
        strategy.RetryCount.Should().Be(retries);
        strategy.BaseDelay.Should().Be(delay);

        assert?.Invoke(strategy);
    }

    [Fact]
    public void AddRetry_InvalidOptions_Throws()
    {
        new CompositeStrategyBuilder()
            .Invoking(b => b.AddRetry(new RetryStrategyOptions { ShouldHandle = null! }))
            .Should()
            .Throw<ValidationException>();

        new CompositeStrategyBuilder<int>()
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

        var strategy = new CompositeStrategyBuilder { Randomizer = () => 1.0 }.AddRetry(new()
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
            }
        })
        .Build();

        // this executes all retries and we aggregate the delays immediately
        strategy.Execute(() => { });

        return aggregatedDelay;
    }
}
