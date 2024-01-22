using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Behavior;
using Polly.Testing;

namespace Polly.Core.Tests.Simmy.Behavior;

public class BehaviorChaosPipelineBuilderExtensionsTests
{
    public static IEnumerable<object[]> AddBehavior_Ok_Data()
    {
        var context = ResilienceContextPool.Shared.Get();
        Func<ValueTask> behavior = () => new ValueTask(Task.CompletedTask);
        yield return new object[]
        {
            (ResiliencePipelineBuilder<int> builder) => { builder.AddChaosBehavior(0.5, behavior); },
            (BehaviorChaosStrategy strategy) =>
            {
                strategy.Behavior!.Invoke(new(context)).Preserve().GetAwaiter().IsCompleted.Should().BeTrue();
                strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().BeTrue();
                strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(0.5);
            }
        };
    }

    [Fact]
    public void AddBehavior_Shortcut_Option_Ok()
    {
        var sut = new ResiliencePipelineBuilder().AddChaosBehavior(0.5, () => new ValueTask(Task.CompletedTask)).Build();
        sut.GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<BehaviorChaosStrategy>();
    }

    [Fact]
    public void AddBehavior_Shortcut_Option_Throws()
    {
        new ResiliencePipelineBuilder()
            .Invoking(b => b.AddChaosBehavior(-1, () => new ValueTask(Task.CompletedTask)))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddBehavior_InvalidOptions_Throws()
    {
        new ResiliencePipelineBuilder()
            .Invoking(b => b.AddChaosBehavior(new ChaosBehaviorStrategyOptions()))
            .Should()
            .Throw<ValidationException>();
    }

    [Fact]
    public void AddBehavior_Options_Ok()
    {
        var sut = new ResiliencePipelineBuilder()
            .AddChaosBehavior(new ChaosBehaviorStrategyOptions
            {
                Enabled = true,
                InjectionRate = 1,
                BehaviorAction = (_) => new ValueTask(Task.CompletedTask)
            })
            .Build();

        sut.GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<BehaviorChaosStrategy>();
    }

    [MemberData(nameof(AddBehavior_Ok_Data))]
    [Theory]
    internal void AddBehavior_Generic_Options_Ok(Action<ResiliencePipelineBuilder<int>> configure, Action<BehaviorChaosStrategy> assert)
    {
        var builder = new ResiliencePipelineBuilder<int>();
        configure(builder);

        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<BehaviorChaosStrategy>().Subject;
        assert(strategy);
    }
}
