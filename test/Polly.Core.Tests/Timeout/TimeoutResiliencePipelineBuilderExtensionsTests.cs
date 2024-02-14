using System.ComponentModel.DataAnnotations;
using Polly.Testing;
using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutResiliencePipelineBuilderExtensionsTests
{
    public static IEnumerable<object[]> AddTimeout_Ok_Data()
    {
        var timeout = TimeSpan.FromSeconds(4);
        yield return new object[]
        {
            timeout,
            (ResiliencePipelineBuilder<int> builder) => { builder.AddTimeout(timeout); },
            (TimeoutResilienceStrategy strategy) => { GetTimeout(strategy).Should().Be(timeout); }
        };
    }

    [MemberData(nameof(TimeoutTestUtils.InvalidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    [Theory]
    public void AddTimeout_InvalidTimeout_EnsureValidated(TimeSpan timeout)
    {
        var builder = new ResiliencePipelineBuilder<int>();

        Assert.Throws<ValidationException>(() => builder.AddTimeout(timeout));
    }

    [MemberData(nameof(AddTimeout_Ok_Data))]
    [Theory]
    internal void AddTimeout_Ok(TimeSpan timeout, Action<ResiliencePipelineBuilder<int>> configure, Action<TimeoutResilienceStrategy> assert)
    {
        var builder = new ResiliencePipelineBuilder<int>();
        configure(builder);

        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<TimeoutResilienceStrategy>().Subject;
        assert(strategy);
        GetTimeout(strategy).Should().Be(timeout);
    }

    [Fact]
    public void AddTimeout_Options_Ok()
    {
        var strategy = new ResiliencePipelineBuilder().AddTimeout(new TimeoutStrategyOptions()).Build();

        strategy.GetPipelineDescriptor().FirstStrategy.StrategyInstance.Should().BeOfType<TimeoutResilienceStrategy>();
    }

    [Fact]
    public void AddTimeout_InvalidOptions_Throws() =>
        new ResiliencePipelineBuilder()
            .Invoking(b => b.AddTimeout(new TimeoutStrategyOptions { Timeout = TimeSpan.Zero }))
            .Should()
            .Throw<ValidationException>();

    private static TimeSpan GetTimeout(TimeoutResilienceStrategy strategy)
    {
        if (strategy.TimeoutGenerator is null)
        {
            return strategy.DefaultTimeout;
        }

        return strategy.TimeoutGenerator(new TimeoutGeneratorArguments(ResilienceContextPool.Shared.Get())).Preserve().GetAwaiter().GetResult();
    }
}
