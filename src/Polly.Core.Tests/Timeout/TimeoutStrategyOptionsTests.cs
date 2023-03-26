using System.ComponentModel.DataAnnotations;
using Polly.Timeout;
using Polly.Utils;
using Xunit;

namespace Polly.Core.Tests.Timeout;

public class TimeoutStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaultValues()
    {
        var options = new TimeoutStrategyOptions();

        options.TimeoutGenerator.Should().NotBeNull();
        options.OnTimeout.Should().NotBeNull();
        options.StrategyType.Should().Be(TimeoutConstants.StrategyType);
    }

    public static readonly TheoryData<TimeSpan> InvalidTimeouts = new()
    {
        TimeSpan.MinValue,
        TimeSpan.Zero,
        TimeSpan.FromSeconds(-1)
    };

    [MemberData(nameof(InvalidTimeouts))]
    [Theory]
    public void Timeout_Invalid_EnsureValidationError(TimeSpan value)
    {
        var options = new TimeoutStrategyOptions
        {
            Timeout = value
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(o, "Dummy message"))
            .Should()
            .Throw<ValidationException>();
    }

    public static readonly TheoryData<TimeSpan> ValidTimeouts = new()
    {
        TimeoutConstants.InfiniteTimeout,
        System.Threading.Timeout.InfiniteTimeSpan,
        TimeSpan.FromSeconds(1)
    };

    [MemberData(nameof(ValidTimeouts))]
    [Theory]
    public void Timeout_Valid(TimeSpan value)
    {
        var options = new TimeoutStrategyOptions
        {
            Timeout = value
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(o, "Dummy message"))
            .Should()
            .NotThrow();
    }
}
