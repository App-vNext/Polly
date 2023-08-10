using System.ComponentModel.DataAnnotations;
using Polly.Timeout;
using Polly.Utils;

namespace Polly.Core.Tests.Timeout;

public class TimeoutCompositeStrategyBuilderExtensionsTests
{
    public static IEnumerable<object[]> AddTimeout_Ok_Data()
    {
        var timeout = TimeSpan.FromSeconds(4);
        yield return new object[]
        {
            timeout,
            (CompositeStrategyBuilder<int> builder) => { builder.AddTimeout(timeout); },
            (TimeoutResilienceStrategy strategy) => { GetTimeout(strategy).Should().Be(timeout); }
        };
    }

    [MemberData(nameof(TimeoutTestUtils.InvalidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    [Theory]
    public void AddTimeout_InvalidTimeout_EnsureValidated(TimeSpan timeout)
    {
        var builder = new CompositeStrategyBuilder<int>();

        Assert.Throws<ValidationException>(() => builder.AddTimeout(timeout));
    }

    [MemberData(nameof(AddTimeout_Ok_Data))]
    [Theory]
    internal void AddTimeout_Ok(TimeSpan timeout, Action<CompositeStrategyBuilder<int>> configure, Action<TimeoutResilienceStrategy> assert)
    {
        var builder = new CompositeStrategyBuilder<int>();
        configure(builder);

        var strategy = ((NonReactiveResilienceStrategyBridge)builder.Build().Strategy).Strategy.Should().BeOfType<TimeoutResilienceStrategy>().Subject;
        assert(strategy);

        GetTimeout(strategy).Should().Be(timeout);
    }

    [Fact]
    public void AddTimeout_Options_Ok()
    {
        var strategy = new CompositeStrategyBuilder().AddTimeout(new TimeoutStrategyOptions()).Build();

        ((NonReactiveResilienceStrategyBridge)strategy).Strategy.Should().BeOfType<TimeoutResilienceStrategy>();
    }

    [Fact]
    public void AddTimeout_InvalidOptions_Throws()
    {
        new CompositeStrategyBuilder()
            .Invoking(b => b.AddTimeout(new TimeoutStrategyOptions { Timeout = TimeSpan.Zero }))
            .Should()
            .Throw<ValidationException>();
    }

    private static TimeSpan GetTimeout(TimeoutResilienceStrategy strategy)
    {
        if (strategy.TimeoutGenerator is null)
        {
            return strategy.DefaultTimeout;
        }

        return strategy.TimeoutGenerator(new TimeoutGeneratorArguments(ResilienceContextPool.Shared.Get())).Preserve().GetAwaiter().GetResult();
    }
}
