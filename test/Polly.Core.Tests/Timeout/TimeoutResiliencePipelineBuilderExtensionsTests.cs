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
            (TimeoutResilienceStrategy strategy) => { GetTimeout(strategy).ShouldBe(timeout); }
        };
    }

    [Theory]
    [MemberData(nameof(TimeoutTestUtils.InvalidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    public void AddTimeout_InvalidTimeout_EnsureValidated(TimeSpan timeout)
    {
        var builder = new ResiliencePipelineBuilder<int>();

        Assert.Throws<ValidationException>(() => builder.AddTimeout(timeout));
    }

    [Theory]
#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [MemberData(nameof(AddTimeout_Ok_Data))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    internal void AddTimeout_Ok(TimeSpan timeout, Action<ResiliencePipelineBuilder<int>> configure, Action<TimeoutResilienceStrategy> assert)
    {
        var builder = new ResiliencePipelineBuilder<int>();
        configure(builder);

        var strategy = builder.Build().GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<TimeoutResilienceStrategy>();
        assert(strategy);
        GetTimeout(strategy).ShouldBe(timeout);
    }

    [Fact]
    public void AddTimeout_Options_Ok()
    {
        var strategy = new ResiliencePipelineBuilder().AddTimeout(new TimeoutStrategyOptions()).Build();

        strategy.GetPipelineDescriptor().FirstStrategy.StrategyInstance.ShouldBeOfType<TimeoutResilienceStrategy>();
    }

    [Fact]
    public void AddTimeout_InvalidOptions_Throws() =>
        Should.Throw<ValidationException>(
            () => new ResiliencePipelineBuilder().AddTimeout(new TimeoutStrategyOptions { Timeout = TimeSpan.Zero }));

    private static TimeSpan GetTimeout(TimeoutResilienceStrategy strategy)
    {
        if (strategy.TimeoutGenerator is null)
        {
            return strategy.DefaultTimeout;
        }

        return strategy.TimeoutGenerator(
            new TimeoutGeneratorArguments(
                ResilienceContextPool.Shared.Get())).Preserve().GetAwaiter().GetResult();
    }
}
