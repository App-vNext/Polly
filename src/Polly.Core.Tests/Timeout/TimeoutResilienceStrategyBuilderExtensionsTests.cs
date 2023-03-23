using System.ComponentModel.DataAnnotations;
using Polly.Builder;
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
            (ResilienceStrategyBuilder builder) => { builder.AddTimeout(timeout); },
            (TimeoutResilienceStrategy strategy) => { GetTimeout(strategy).Should().Be(timeout); }
        };

        bool called = false;

        yield return new object[]
        {
            timeout,
            (ResilienceStrategyBuilder builder) => { builder.AddTimeout(timeout, _=> called = true); },
            (TimeoutResilienceStrategy strategy) =>
            {
                GetTimeout(strategy).Should().Be(timeout);
                OnTimeout(strategy);
                called.Should().BeTrue();
            }
        };
    }

    [InlineData(-1)]
    [InlineData(0)]
    [Theory]
    public void AddTimeout_InvalidTimeout_EnsureValidated(int value)
    {
        var timeout = value switch
        {
            0 => TimeSpan.Zero,
            -1 => TimeSpan.FromSeconds(-1),
            _ => throw new NotSupportedException()
        };
        var builder = new ResilienceStrategyBuilder();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddTimeout(timeout));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddTimeout(timeout, args => { }));
    }

    [MemberData(nameof(AddTimeout_Ok_Data))]
    [Theory]
    internal async Task AddTimeout_Ok(TimeSpan timeout, Action<ResilienceStrategyBuilder> configure, Action<TimeoutResilienceStrategy> assert)
    {
        var builder = new ResilienceStrategyBuilder();
        configure(builder);
        var strategy = builder.Build().Should().BeOfType<TimeoutResilienceStrategy>().Subject;
        assert(strategy);

        (await strategy.TimeoutGenerator(new TimeoutGeneratorArguments(ResilienceContext.Get()))).Should().Be(timeout);
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
            .Invoking(b => b.AddTimeout(new TimeoutStrategyOptions { TimeoutGenerator = null! }))
            .Should()
            .Throw<ValidationException>().WithMessage("The timeout strategy options are invalid.*");
    }

    private static TimeSpan GetTimeout(TimeoutResilienceStrategy strategy) => strategy.TimeoutGenerator(new TimeoutGeneratorArguments(ResilienceContext.Get())).Preserve().Result;

    private static void OnTimeout(TimeoutResilienceStrategy strategy)
    {
        strategy.OnTimeout?.Invoke(TimeoutTestUtils.OnTimeoutArguments()).AsTask().Wait();
    }
}
