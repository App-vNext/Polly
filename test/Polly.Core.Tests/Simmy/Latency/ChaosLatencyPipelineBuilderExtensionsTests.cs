using Polly.Simmy;
using Polly.Simmy.Latency;
using Polly.Testing;

namespace Polly.Core.Tests.Simmy.Latency;

public class ChaosLatencyPipelineBuilderExtensionsTests
{
    public static IEnumerable<object[]> AddLatency_Ok_Data()
    {
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);
        Func<ValueTask> behavior = () => new ValueTask(Task.CompletedTask);
        yield return new object[]
        {
            (ResiliencePipelineBuilder<int> builder) => { builder.AddChaosLatency(0.5, TimeSpan.FromSeconds(10)); },
            (ChaosLatencyStrategy strategy) =>
            {
                strategy.Latency.ShouldBe(TimeSpan.FromSeconds(10));
                strategy.LatencyGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(TimeSpan.FromSeconds(10));
                strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBeTrue();
                strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().ShouldBe(0.5);
            }
        };
    }

    [Fact]
    public void AddLatency_Shortcut_Option_Ok()
    {
        var sut = new ResiliencePipelineBuilder().AddChaosLatency(0.5, TimeSpan.FromSeconds(10)).Build();
        sut.GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<ChaosLatencyStrategy>();
    }

    [Fact]
    public void AddLatency_Options_Ok()
    {
        var sut = new ResiliencePipelineBuilder()
            .AddChaosLatency(new ChaosLatencyStrategyOptions
            {
                InjectionRate = 1,
                LatencyGenerator = (_) => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(30))
            })
            .Build();

        sut.GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<ChaosLatencyStrategy>();
    }

    [MemberData(nameof(AddLatency_Ok_Data))]
    [Theory]
    internal void AddLatency_Generic_Options_Ok(Action<ResiliencePipelineBuilder<int>> configure, Action<ChaosLatencyStrategy> assert)
    {
        var builder = new ResiliencePipelineBuilder<int>();
        configure(builder);

        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<ChaosLatencyStrategy>();
        assert(strategy);
    }
}
