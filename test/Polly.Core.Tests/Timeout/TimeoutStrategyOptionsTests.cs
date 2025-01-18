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

        options.TimeoutGenerator.ShouldBeNull();
        options.OnTimeout.ShouldBeNull();
        options.Name.ShouldBe("Timeout");
    }

    [MemberData(nameof(TimeoutTestUtils.InvalidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    [Theory]
    public void Timeout_Invalid_EnsureValidationError(TimeSpan value)
    {
        var options = new TimeoutStrategyOptions
        {
            Timeout = value
        };

        Should.Throw<ValidationException>(
            () => ValidationHelper.ValidateObject(new(options, "Dummy message")));
    }

    [MemberData(nameof(TimeoutTestUtils.ValidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    [Theory]
    public void Timeout_Valid(TimeSpan value)
    {
        var options = new TimeoutStrategyOptions
        {
            Timeout = value
        };

        Should.NotThrow(
            () => ValidationHelper.ValidateObject(new(options, "Dummy message")));
    }
}
