using Polly.Simmy;
using Polly.Simmy.Latency;
using Polly.Testing;

namespace Polly.Core.Tests.Simmy.Latency;

public class LatencyChaosPipelineBuilderExtensionsTests
{
    public static IEnumerable<object[]> AddLatency_Ok_Data()
    {
        var context = ResilienceContextPool.Shared.Get();
        Func<ValueTask> behavior = () => new ValueTask(Task.CompletedTask);
        yield return new object[]
        {
            (ResiliencePipelineBuilder<int> builder) => { builder.AddChaosLatency(true, 0.5, TimeSpan.FromSeconds(10)); },
            (LatencyChaosStrategy strategy) =>
            {
                strategy.LatencyGenerator!.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(TimeSpan.FromSeconds(10));
                strategy.EnabledGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().BeTrue();
                strategy.InjectionRateGenerator.Invoke(new(context)).Preserve().GetAwaiter().GetResult().Should().Be(0.5);
            }
        };
    }

    [Fact]
    public void AddLatency_Shortcut_Option_Ok()
    {
        var sut = new ResiliencePipelineBuilder().AddChaosLatency(true, 0.5, TimeSpan.FromSeconds(10)).Build();
        sut.GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<LatencyChaosStrategy>();
    }

    [Fact]
    public void AddLatency_Options_Ok()
    {
        var sut = new ResiliencePipelineBuilder()
            .AddChaosLatency(new LatencyStrategyOptions
            {
                Enabled = true,
                InjectionRate = 1,
                LatencyGenerator = (_) => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(30))
            })
            .Build();

        sut.GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<LatencyChaosStrategy>();
    }

    [MemberData(nameof(AddLatency_Ok_Data))]
    [Theory]
    internal void AddLatency_Generic_Options_Ok(Action<ResiliencePipelineBuilder<int>> configure, Action<LatencyChaosStrategy> assert)
    {
        var builder = new ResiliencePipelineBuilder<int>();
        configure(builder);

        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<LatencyChaosStrategy>().Subject;
        assert(strategy);
    }
}
