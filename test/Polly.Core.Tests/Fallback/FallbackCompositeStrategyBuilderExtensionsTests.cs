using System.ComponentModel.DataAnnotations;
using Polly.Fallback;
using Polly.Testing;

namespace Polly.Core.Tests.Fallback;

public class FallbackCompositeStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<CompositeStrategyBuilder<int>>> FallbackOverloadsGeneric = new()
    {
        builder =>
        {
            builder.AddFallback(new FallbackStrategyOptions<int>
            {
                FallbackAction = _ => Outcome.FromResultAsTask(0),
                ShouldHandle = _ => PredicateResult.False,
            });
        }
    };

    [MemberData(nameof(FallbackOverloadsGeneric))]
    [Theory]
    public void AddFallback_Generic_Ok(Action<CompositeStrategyBuilder<int>> configure)
    {
        var builder = new CompositeStrategyBuilder<int>();
        configure(builder);

        builder.Build().GetInnerStrategies().FirstStrategy.StrategyInstance.Should().BeOfType(typeof(FallbackResilienceStrategy<int>));
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
            FallbackAction = _ => Outcome.FromResultAsTask((object)1)
        };

        var strategy = new CompositeStrategyBuilder().AddFallback(options).Build();

        strategy.Execute<int>(_ => -1).Should().Be(1);
        strategy.Execute<int>(_ => throw new InvalidOperationException()).Should().Be(1);
    }

    [Fact]
    public void AddFallback_InvalidOptions_Throws()
    {
        new CompositeStrategyBuilder()
            .Invoking(b => b.AddFallback(new FallbackStrategyOptions()))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddFallbackT_InvalidOptions_Throws()
    {
        new CompositeStrategyBuilder<double>()
            .Invoking(b => b.AddFallback(new FallbackStrategyOptions<double>()))
            .Should()
            .Throw<ValidationException>();
    }
}
