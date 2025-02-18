using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Outcomes;
using Polly.Testing;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class ChaosOutcomePipelineBuilderExtensionsTests
{
#pragma warning disable IDE0028
    public static readonly List<Action<ResiliencePipelineBuilder<int>>> ResultStrategy = new()
    {
        builder =>
        {
            var options = new ChaosOutcomeStrategyOptions<int>
            {
                InjectionRate = 0.6,
                Randomizer = () => 0.5,
                OutcomeGenerator = (_) => new ValueTask<Outcome<int>?>(Outcome.FromResult(100))
            };
            builder.AddChaosOutcome(options);

            AssertResultStrategy(builder, options, true, 0.6, new(100));
        },
    };
#pragma warning restore IDE0028

    private static void AssertResultStrategy<T>(ResiliencePipelineBuilder<T> builder, ChaosOutcomeStrategyOptions<T> options, bool enabled, double injectionRate, Outcome<T> outcome)
    {
        var context = ResilienceContextPool.Shared.Get();
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<ChaosOutcomeStrategy<T>>();

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(injectionRate);
        options.OutcomeGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(outcome);
    }

    [Theory]
#pragma warning disable xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    //[MemberData(nameof(ResultStrategy))]
    [InlineData(0)]
#pragma warning restore xUnit1044 // Avoid using TheoryData type arguments that are not serializable
    internal void AddResult_Options_Ok(int index)
    {
        Action<ResiliencePipelineBuilder<int>> configure = ResultStrategy[index];
        var builder = new ResiliencePipelineBuilder<int>();
        Should.NotThrow(() => configure(builder));
    }

    [Fact]
    public void AddResult_Shortcut_Generator_Option_Ok()
    {
        var builder = new ResiliencePipelineBuilder<int>();
        var pipeline = builder
            .AddChaosOutcome(0.5, () => 120)
            .Build();

        var descriptor = pipeline.GetPipelineDescriptor();
        var options = Assert.IsType<ChaosOutcomeStrategyOptions<int>>(descriptor.Strategies[0].Options);

        AssertResultStrategy(builder, options, true, 0.5, new(120));
    }

    [Fact]
    public void AddResult_Shortcut_Option_Throws() =>
        Should.Throw<ValidationException>(
            () => new ResiliencePipelineBuilder<int>().AddChaosOutcome(-1, () => 120));
}
