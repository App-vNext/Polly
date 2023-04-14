using System.ComponentModel.DataAnnotations;
using Polly.Builder;
using Polly.Retry;

namespace Polly.Core.Tests.Retry;

#pragma warning disable CA2012 // Use ValueTasks correctly

public class RetryResilienceStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<ResilienceStrategyBuilder>> OverloadsData = new()
    {
        builder =>
        {
            builder.AddRetry(retry=>retry.HandleResult(10));
            AssertStrategy(builder, RetryBackoffType.Exponential, 3, TimeSpan.FromSeconds(2));
        },
        builder =>
        {
            builder.AddRetry(retry=>retry.HandleResult(10), RetryBackoffType.Linear);
            AssertStrategy(builder, RetryBackoffType.Linear, 3, TimeSpan.FromSeconds(2));
        },
        builder =>
        {
            builder.AddRetry(retry=>retry.HandleResult(10), RetryBackoffType.Linear, 2, TimeSpan.FromSeconds(1));
            AssertStrategy(builder, RetryBackoffType.Linear, 2, TimeSpan.FromSeconds(1));
        },
        builder =>
        {
            builder.AddRetry(retry => retry.HandleResult(10), attempt => TimeSpan.FromMilliseconds(attempt));

            AssertStrategy(builder, RetryBackoffType.Exponential, 3, TimeSpan.FromSeconds(2), strategy =>
            {
                var args = new RetryDelayArguments(ResilienceContext.Get(), 8, TimeSpan.Zero);

                strategy.DelayGenerator!.GenerateAsync(new Polly.Strategy.Outcome<bool>(new InvalidOperationException()), args).Result.Should().Be(TimeSpan.FromMilliseconds(8));
            });
        },
    };

    [MemberData(nameof(OverloadsData))]
    [Theory]
    public void AddRetry_Overloads_Ok(Action<ResilienceStrategyBuilder> configure)
    {
        var builder = new ResilienceStrategyBuilder();
        var options = new RetryStrategyOptions();

        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [Fact]
    public void AddRetry_DefaultOptions_Ok()
    {
        var builder = new ResilienceStrategyBuilder();
        var options = new RetryStrategyOptions();

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

    [Fact]
    public void AddRetry_InvalidOptions_Throws()
    {
        var builder = new ResilienceStrategyBuilder();

        builder
            .Invoking(b => b.AddRetry(new RetryStrategyOptions { ShouldRetry = null! }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The retry strategy options are invalid.*");
    }
}
