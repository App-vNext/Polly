using System.ComponentModel.DataAnnotations;
using Polly.Fallback;

namespace Polly.Core.Tests.Fallback;

public class FallbackResilienceStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<ResilienceStrategyBuilder<int>>> FallbackOverloadsGeneric = new()
    {
        builder =>
        {
            builder.AddFallback(new FallbackStrategyOptions<int>
            {
                FallbackAction = _ =>  0.AsOutcomeAsync(),
                ShouldHandle = _ => PredicateResult.False,
            });
        },
        builder =>
        {
            builder.AddFallback(handle => handle.HandleResult(1), _ =>  0.AsOutcomeAsync());
        },
    };

    [MemberData(nameof(FallbackOverloadsGeneric))]
    [Theory]
    public void AddFallback_Generic_Ok(Action<ResilienceStrategyBuilder<int>> configure)
    {
        var builder = new ResilienceStrategyBuilder<int>();
        configure(builder);
        builder.Build().Strategy.Should().BeOfType<FallbackResilienceStrategy<int>>();
    }

    [Fact]
    public void AddFallback_Ok()
    {
        var options = new FallbackStrategyOptions
        {
            ShouldHandle = args => args switch
            {
                { Exception: InvalidOperationException } => PredicateResult.True,
                { Result: -1 } => PredicateResult.True,
                _ => PredicateResult.False
            },
            FallbackAction = _ => ((object)1).AsOutcomeAsync()
        };

        var strategy = new ResilienceStrategyBuilder().AddFallback(options).Build();

        strategy.Execute<int>(_ => -1).Should().Be(1);
        strategy.Execute<int>(_ => throw new InvalidOperationException()).Should().Be(1);
    }

    [Fact]
    public void AddFallback_InvalidOptions_Throws()
    {
        new ResilienceStrategyBuilder()
            .Invoking(b => b.AddFallback(new FallbackStrategyOptions()))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddFallbackT_InvalidOptions_Throws()
    {
        new ResilienceStrategyBuilder<double>()
            .Invoking(b => b.AddFallback(new FallbackStrategyOptions<double>()))
            .Should()
            .Throw<ValidationException>();
    }
}
