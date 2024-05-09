using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Outcomes;
using Polly.Testing;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class ChaosOutcomePipelineBuilderExtensionsTests
{
#pragma warning disable IDE0028
    public static readonly TheoryData<Action<ResiliencePipelineBuilder<int>>> ResultStrategy = new()
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
        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<ChaosOutcomeStrategy<T>>().Subject;

        strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(enabled);
        strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(injectionRate);
        options.OutcomeGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(outcome);
    }

    [MemberData(nameof(ResultStrategy))]
    [Theory]
    internal void AddResult_Options_Ok(Action<ResiliencePipelineBuilder<int>> configure)
    {
        var builder = new ResiliencePipelineBuilder<int>();
        builder.Invoking(b => configure(b)).Should().NotThrow();
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
        new ResiliencePipelineBuilder<int>()
            .Invoking(b => b.AddChaosOutcome(-1, () => 120))
            .Should()
            .Throw<ValidationException>();
}
