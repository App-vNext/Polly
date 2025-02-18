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

    //[MemberData(nameof(TimeoutTestUtils.InvalidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [Theory]
    public void Timeout_Invalid_EnsureValidationError(int index)
    {
        TimeSpan value = TimeoutTestUtils.InvalidTimeouts[index];
        var options = new TimeoutStrategyOptions
        {
            Timeout = value
        };

        Should.Throw<ValidationException>(
            () => ValidationHelper.ValidateObject(new(options, "Dummy message")));
    }

    //[MemberData(nameof(TimeoutTestUtils.ValidTimeouts), MemberType = typeof(TimeoutTestUtils))]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [Theory]
    public void Timeout_Valid(int index)
    {
        TimeSpan value = TimeoutTestUtils.ValidTimeouts[index];
        var options = new TimeoutStrategyOptions
        {
            Timeout = value
        };

        Should.NotThrow(
            () => ValidationHelper.ValidateObject(new(options, "Dummy message")));
    }
}
