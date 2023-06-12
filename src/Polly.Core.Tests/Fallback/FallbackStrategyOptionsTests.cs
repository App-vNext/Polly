using System.ComponentModel.DataAnnotations;
using Polly.Fallback;
using Polly.Utils;

namespace Polly.Core.Tests.Fallback;

public class FallbackStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new FallbackStrategyOptions<int>();

        options.StrategyType.Should().Be("Fallback");
        options.ShouldHandle.Should().BeNull();
        options.OnFallback.Should().BeNull();
        options.FallbackAction.Should().BeNull();
    }

    [Fact]
    public void Validation()
    {
        var options = new FallbackStrategyOptions<int>
        {
            ShouldHandle = null!
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(o, "Invalid."))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid.

            Validation Errors:
            The ShouldHandle field is required.
            The FallbackAction field is required.
            """);
    }
}
