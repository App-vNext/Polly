using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Behavior;
using Polly.Testing;

namespace Polly.Core.Tests.Simmy.Behavior;

public class ChaosBehaviorPipelineBuilderExtensionsTests
{
    public static IEnumerable<object[]> AddBehavior_Ok_Data()
    {
        var context = ResilienceContextPool.Shared.Get();
        Func<CancellationToken, ValueTask> behavior = _ => default;
        yield return new object[]
        {
            (ResiliencePipelineBuilder<int> builder) => { builder.AddChaosBehavior(0.5, behavior); },
            (ChaosBehaviorStrategy strategy) =>
            {
                strategy.Behavior!.Invoke(new(context)).Preserve().GetAwaiter().IsCompleted.ShouldBeTrue();
                strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBeTrue();
                strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(0.5);
            }
        };
    }

    [Fact]
    public void AddBehavior_Shortcut_Option_Ok()
    {
        var sut = new ResiliencePipelineBuilder().AddChaosBehavior(0.5, _ => default).Build();
        sut.GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<ChaosBehaviorStrategy>();
    }

    [Fact]
    public void AddBehavior_Shortcut_Option_Throws() =>
        Should.Throw<ValidationException>(() => new ResiliencePipelineBuilder().AddChaosBehavior(-1, _ => default));

    [Fact]
    public void AddBehavior_InvalidOptions_Throws() =>
        Should.Throw<ValidationException>(() => new ResiliencePipelineBuilder().AddChaosBehavior(new ChaosBehaviorStrategyOptions()));

    [Fact]
    public void AddBehavior_Options_Ok()
    {
        var sut = new ResiliencePipelineBuilder()
            .AddChaosBehavior(new ChaosBehaviorStrategyOptions
            {
                InjectionRate = 1,
                BehaviorGenerator = (_) => default
            })
            .Build();

        sut.GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<ChaosBehaviorStrategy>();
    }

    [MemberData(nameof(AddBehavior_Ok_Data))]
    [Theory]
    internal void AddBehavior_Generic_Options_Ok(Action<ResiliencePipelineBuilder<int>> configure, Action<ChaosBehaviorStrategy> assert)
    {
        var builder = new ResiliencePipelineBuilder<int>();
        configure(builder);

        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<ChaosBehaviorStrategy>();
        assert(strategy);
    }
}
