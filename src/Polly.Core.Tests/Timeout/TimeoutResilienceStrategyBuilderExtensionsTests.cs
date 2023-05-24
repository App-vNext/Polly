using System.ComponentModel.DataAnnotations;
using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutResilienceStrategyBuilderExtensionsTests
{
    public static IEnumerable<object[]> AddTimeout_Ok_Data()
    {
        var timeout = TimeSpan.FromSeconds(4);

        yield return new object[]
        {
            timeout,
            (ResilienceStrategyBuilder<int> builder) => { builder.AddTimeout(timeout); },
            (TimeoutResilienceStrategy strategy) => { GetTimeout(strategy).Should().Be(timeout); }
        };

        bool called = false;

        yield return new object[]
        {
            timeout,
            (ResilienceStrategyBuilder<int> builder) => { builder.AddTimeout(timeout, _=> called = true); },
            (TimeoutResilienceStrategy strategy) =>
            {
                GetTimeout(strategy).Should().Be(timeout);
                OnTimeout(strategy);
                called.Should().BeTrue();
            }
        };
    }

    [MemberData(nameof(TimeoutTestUtils.InvalidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    [Theory]
    public void AddTimeout_InvalidTimeout_EnsureValidated(TimeSpan timeout)
    {
        var builder = new ResilienceStrategyBuilder<int>();

        Assert.Throws<ValidationException>(() => builder.AddTimeout(timeout));
        Assert.Throws<ValidationException>(() => builder.AddTimeout(timeout, args => { }));
    }

    [MemberData(nameof(AddTimeout_Ok_Data))]
    [Theory]
    internal void AddTimeout_Ok(TimeSpan timeout, Action<ResilienceStrategyBuilder<int>> configure, Action<TimeoutResilienceStrategy> assert)
    {
        var builder = new ResilienceStrategyBuilder<int>();
        configure(builder);
        var strategy = builder.Build().Strategy.Should().BeOfType<TimeoutResilienceStrategy>().Subject;
        assert(strategy);

        GetTimeout(strategy).Should().Be(timeout);
    }

    [Fact]
    public void AddTimeout_Options_Ok()
    {
        var strategy = new ResilienceStrategyBuilder().AddTimeout(new TimeoutStrategyOptions()).Build();

        strategy.Should().BeOfType<TimeoutResilienceStrategy>();
    }

    [Fact]
    public void AddTimeout_InvalidOptions_Throws()
    {
        new ResilienceStrategyBuilder()
            .Invoking(b => b.AddTimeout(new TimeoutStrategyOptions { Timeout = TimeSpan.Zero }))
            .Should()
            .Throw<ValidationException>().WithMessage("The timeout strategy options are invalid.*");
    }

    private static TimeSpan GetTimeout(TimeoutResilienceStrategy strategy)
    {
        if (strategy.TimeoutGenerator == null)
        {
            return strategy.DefaultTimeout;
        }

        return strategy.GenerateTimeoutAsync(ResilienceContext.Get()).Preserve().GetAwaiter().GetResult();
    }

    private static void OnTimeout(TimeoutResilienceStrategy strategy)
    {
        strategy.OnTimeout?.Invoke(TimeoutTestUtils.OnTimeoutArguments()).AsTask().Wait();
    }
}
