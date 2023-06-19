using System.ComponentModel.DataAnnotations;
using Polly.Retry;

namespace Polly.Core.Tests.Retry;

#pragma warning disable CA2012 // Use ValueTasks correctly

public class RetryResilienceStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<ResilienceStrategyBuilder>> OverloadsData = new()
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

    public static readonly TheoryData<Action<ResilienceStrategyBuilder<int>>> OverloadsDataGeneric = new()
    {
        builder =>
        {
            builder.AddRetry(retry => retry.HandleResult(10), RetryBackoffType.Linear, 2, TimeSpan.FromSeconds(1));
            AssertStrategy(builder, RetryBackoffType.Linear, 2, TimeSpan.FromSeconds(1));
        },
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
    public void AddRetry_Overloads_Ok(Action<ResilienceStrategyBuilder> configure)
    {
        var builder = new ResilienceStrategyBuilder();

        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [MemberData(nameof(OverloadsDataGeneric))]
    [Theory]
    public void AddRetry_GenericOverloads_Ok(Action<ResilienceStrategyBuilder<int>> configure)
    {
        var builder = new ResilienceStrategyBuilder<int>();

        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [Fact]
    public void AddRetry_DefaultOptions_Ok()
    {
        var builder = new ResilienceStrategyBuilder();
        var options = new RetryStrategyOptions { ShouldHandle = _ => PredicateResult.True };

        builder.AddRetry(options);

        AssertStrategy(builder, options.BackoffType, options.RetryCount, options.BaseDelay);
    }

    private static void AssertStrategy(ResilienceStrategyBuilder builder, RetryBackoffType type, int retries, TimeSpan delay, Action<RetryResilienceStrategy>? assert = null)
    {
        var strategy = (RetryResilienceStrategy)builder.Build();

        strategy.BackoffType.Should().Be(type);
        strategy.RetryCount.Should().Be(retries);
        strategy.BaseDelay.Should().Be(delay);

        assert?.Invoke(strategy);
    }

    private static void AssertStrategy<T>(ResilienceStrategyBuilder<T> builder, RetryBackoffType type, int retries, TimeSpan delay, Action<RetryResilienceStrategy>? assert = null)
    {
        var strategy = (RetryResilienceStrategy)builder.Build().Strategy;

        strategy.BackoffType.Should().Be(type);
        strategy.RetryCount.Should().Be(retries);
        strategy.BaseDelay.Should().Be(delay);

        assert?.Invoke(strategy);
    }

    [Fact]
    public void AddRetry_InvalidOptions_Throws()
    {
        new ResilienceStrategyBuilder()
            .Invoking(b => b.AddRetry(new RetryStrategyOptions { ShouldHandle = null! }))
            .Should()
            .Throw<ValidationException>();

        new ResilienceStrategyBuilder<int>()
            .Invoking(b => b.AddRetry(new RetryStrategyOptions<int> { ShouldHandle = null! }))
            .Should()
            .Throw<ValidationException>();
    }
}
