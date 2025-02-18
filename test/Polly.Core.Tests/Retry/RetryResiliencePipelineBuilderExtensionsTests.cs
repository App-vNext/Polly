using System.ComponentModel.DataAnnotations;
using NSubstitute;
using Polly.Retry;
using Polly.Testing;

namespace Polly.Core.Tests.Retry;

public class RetryResiliencePipelineBuilderExtensionsTests
{
#pragma warning disable IDE0028
    public static readonly List<Action<ResiliencePipelineBuilder>> OverloadsData = new()
    {
        builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = _ => PredicateResult.True(),
            });

            AssertStrategy(builder, DelayBackoffType.Exponential, 3, TimeSpan.FromSeconds(2));
        },
    };

    public static readonly List<Action<ResiliencePipelineBuilder<int>>> OverloadsDataGeneric = new()
    {
        builder =>
        {
            builder.AddRetry(new RetryStrategyOptions<int>
            {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = _ => PredicateResult.True()
            });

            AssertStrategy(builder, DelayBackoffType.Exponential, 3, TimeSpan.FromSeconds(2));
        },
    };
#pragma warning restore IDE0028

    //[MemberData(nameof(OverloadsData))]
    [InlineData(0)]
    [Theory]
    public void AddRetry_Overloads_Ok(int index)
    {
        Action<ResiliencePipelineBuilder> configure = OverloadsData[index];
        var builder = new ResiliencePipelineBuilder();

        Should.NotThrow(() => configure(builder));
    }

    //[MemberData(nameof(OverloadsDataGeneric))]
    [InlineData(0)]
    [Theory]
    public void AddRetry_GenericOverloads_Ok(int index)
    {
        Action<ResiliencePipelineBuilder<int>> configure = OverloadsDataGeneric[index];
        var builder = new ResiliencePipelineBuilder<int>();

        Should.NotThrow(() => configure(builder));
    }

    [Fact]
    public void AddRetry_DefaultOptions_Ok()
    {
        var builder = new ResiliencePipelineBuilder();
        var options = new RetryStrategyOptions { ShouldHandle = _ => PredicateResult.True() };

        builder.AddRetry(options);

        AssertStrategy(builder, options.BackoffType, options.MaxRetryAttempts, options.Delay);
    }

    private static void AssertStrategy(ResiliencePipelineBuilder builder, DelayBackoffType type, int retries, TimeSpan delay, Action<RetryResilienceStrategy<object>>? assert = null)
    {
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<RetryResilienceStrategy<object>>();

        strategy.BackoffType.ShouldBe(type);
        strategy.RetryCount.ShouldBe(retries);
        strategy.BaseDelay.ShouldBe(delay);

        assert?.Invoke(strategy);
    }

    private static void AssertStrategy<T>(
        ResiliencePipelineBuilder<T> builder,
        DelayBackoffType type,
        int retries,
        TimeSpan delay,
        Action<RetryResilienceStrategy<T>>? assert = null)
    {
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<RetryResilienceStrategy<T>>();

        strategy.BackoffType.ShouldBe(type);
        strategy.RetryCount.ShouldBe(retries);
        strategy.BaseDelay.ShouldBe(delay);

        assert?.Invoke(strategy);
    }

    [Fact]
    public void AddRetry_InvalidOptions_Throws()
    {
        Should.Throw<ValidationException>(() => new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions { ShouldHandle = null! }));
        Should.Throw<ValidationException>(() => new ResiliencePipelineBuilder<int>().AddRetry(new RetryStrategyOptions<int> { ShouldHandle = null! }));
    }

    [Fact]
    public void GetAggregatedDelay_ShouldReturnTheSameValue()
    {
        var options = new RetryStrategyOptions { BackoffType = DelayBackoffType.Exponential, UseJitter = true };

        var delay = GetAggregatedDelay(options);
        GetAggregatedDelay(options).ShouldBe(delay);
    }

    [Fact]
    public void GetAggregatedDelay_EnsureCorrectValue()
    {
        var options = new RetryStrategyOptions { BackoffType = DelayBackoffType.Constant, Delay = TimeSpan.FromSeconds(1), MaxRetryAttempts = 5 };

        GetAggregatedDelay(options).ShouldBe(TimeSpan.FromSeconds(5));
    }

    private static TimeSpan GetAggregatedDelay<T>(RetryStrategyOptions<T> options)
    {
        var aggregatedDelay = TimeSpan.Zero;

        var strategy = new ResiliencePipelineBuilder { TimeProvider = new NoWaitingTimeProvider() }.AddRetry(new()
        {
            MaxRetryAttempts = options.MaxRetryAttempts,
            Delay = options.Delay,
            BackoffType = options.BackoffType,
            ShouldHandle = _ => PredicateResult.True(), // always retry until all retries are exhausted
            OnRetry = args =>
            {
                // the delay hint is calculated for this attempt by the retry strategy
                aggregatedDelay += args.RetryDelay;

                return default;
            },
            Randomizer = () => 1.0,
        })
        .Build();

        // this executes all retries and we aggregate the delays immediately
        strategy.Execute(() => { });

        return aggregatedDelay;
    }

    private class NoWaitingTimeProvider : TimeProvider
    {
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            callback(state);
            return Substitute.For<ITimer>();
        }
    }
}
