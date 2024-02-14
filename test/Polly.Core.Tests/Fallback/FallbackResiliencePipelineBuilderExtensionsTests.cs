using System.ComponentModel.DataAnnotations;
using Polly.Fallback;
using Polly.Testing;

namespace Polly.Core.Tests.Fallback;

public class FallbackResiliencePipelineBuilderExtensionsTests
{
#pragma warning disable IDE0028
    public static readonly TheoryData<Action<ResiliencePipelineBuilder<int>>> FallbackOverloadsGeneric = new()
    {
        builder =>
        {
            builder.AddFallback(new FallbackStrategyOptions<int>
            {
                FallbackAction = _ => Outcome.FromResultAsValueTask(0),
                ShouldHandle = _ => PredicateResult.False(),
            });
        }
    };
#pragma warning restore IDE0028

    [MemberData(nameof(FallbackOverloadsGeneric))]
    [Theory]
    public void AddFallback_Generic_Ok(Action<ResiliencePipelineBuilder<int>> configure)
    {
        var builder = new ResiliencePipelineBuilder<int>();
        configure(builder);

        builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType(typeof(FallbackResilienceStrategy<int>));
    }

    [Fact]
    public void AddFallbackT_InvalidOptions_Throws() =>
        new ResiliencePipelineBuilder<double>()
            .Invoking(b => b.AddFallback(new FallbackStrategyOptions<double>()))
            .Should()
            .Throw<ValidationException>();
}
