using System.ComponentModel.DataAnnotations;

namespace Polly.Core.Tests.Utils;

public class TimeSpanAttributeTests
{
    [Fact]
    public void InvalidValue_Skipped()
    {
        var attr = new TimeSpanAttribute("00:00:01");

        attr.GetValidationResult(new object(), new ValidationContext(TimeSpan.FromSeconds(1)) { DisplayName = "A" })
            .Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void InvalidMinValue_Validated()
    {
        var attr = new TimeSpanAttribute("00:00:01");

        attr.GetValidationResult(TimeSpan.FromSeconds(0), new ValidationContext(TimeSpan.FromSeconds(0)) { DisplayName = "A" })!
            .ErrorMessage.Should().Be("The field A must be >= to 00:00:01.");

        attr.GetValidationResult(TimeSpan.FromSeconds(1), new ValidationContext(TimeSpan.FromSeconds(1)) { DisplayName = "A" })
            .Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void InvalidMaxValue_Validated()
    {
        var attr = new TimeSpanAttribute("00:00:01", "00:00:03");

        attr
            .GetValidationResult(TimeSpan.FromSeconds(0), new ValidationContext(TimeSpan.FromSeconds(0)) { DisplayName = "A" })!
            .ErrorMessage.Should().Be("The field A must be >= to 00:00:01.");
        attr.GetValidationResult(TimeSpan.FromSeconds(1), new ValidationContext(TimeSpan.FromSeconds(1)) { DisplayName = "A" })
            .Should().Be(ValidationResult.Success);

        attr
            .GetValidationResult(TimeSpan.FromSeconds(4), new ValidationContext(TimeSpan.FromSeconds(4)) { DisplayName = "A" })!
            .ErrorMessage.Should().Be("The field A must be <= to 00:00:03.");
        attr.GetValidationResult(TimeSpan.FromSeconds(3), new ValidationContext(TimeSpan.FromSeconds(3)) { DisplayName = "A" })
            .Should().Be(ValidationResult.Success);
    }
}
