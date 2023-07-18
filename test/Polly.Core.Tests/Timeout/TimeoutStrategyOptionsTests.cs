using System.ComponentModel.DataAnnotations;
using Polly.Timeout;
using Polly.Utils;

namespace Polly.Core.Tests.Timeout;

public class TimeoutStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaultValues()
    {
        var options = new TimeoutStrategyOptions();

        options.TimeoutGenerator.Should().BeNull();
        options.OnTimeout.Should().BeNull();
        options.StrategyType.Should().Be(TimeoutConstants.StrategyType);
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
            .Invoking(o => ValidationHelper.ValidateObject(new(o, "Dummy message")))
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
            .Invoking(o => ValidationHelper.ValidateObject(new(o, "Dummy message")))
            .Should()
            .NotThrow();
    }
}
