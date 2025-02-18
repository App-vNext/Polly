using System.ComponentModel.DataAnnotations;
using Polly.Fallback;
using Polly.Testing;

namespace Polly.Core.Tests.Fallback;

public class FallbackResiliencePipelineBuilderExtensionsTests
{
#pragma warning disable IDE0028
    public static readonly List<Action<ResiliencePipelineBuilder<int>>> FallbackOverloadsGeneric = new()
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

    //[MemberData(nameof(FallbackOverloadsGeneric))]
    [InlineData(0)]
    [Theory]
    public void AddFallback_Generic_Ok(int index)
    {
        Action<ResiliencePipelineBuilder<int>> configure = FallbackOverloadsGeneric[index];

        var builder = new ResiliencePipelineBuilder<int>();
        configure(builder);

        builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<FallbackResilienceStrategy<int>>();
    }

    [Fact]
    public void AddFallbackT_InvalidOptions_Throws()
        => Should.Throw<ValidationException>(() => new ResiliencePipelineBuilder<double>().AddFallback(new FallbackStrategyOptions<double>()));
}
