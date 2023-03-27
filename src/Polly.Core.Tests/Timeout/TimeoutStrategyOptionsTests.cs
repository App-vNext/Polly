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
        options.Timeout.Should().Be(TimeoutStrategyOptions.InfiniteTimeout);

        TimeoutStrategyOptions.InfiniteTimeout.Should().Be(System.Threading.Timeout.InfiniteTimeSpan);
    }

    [MemberData(nameof(TimeoutTestUtils.InvalidTimeouts), MemberType = typeof(TimeoutTestUtils))]
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

    [MemberData(nameof(TimeoutTestUtils.ValidTimeouts), MemberType = typeof(TimeoutTestUtils))]
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
