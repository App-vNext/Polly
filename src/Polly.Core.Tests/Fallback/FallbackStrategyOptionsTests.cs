using System.ComponentModel.DataAnnotations;
using Polly.Fallback;
using Polly.Utils;

namespace Polly.Core.Tests.Fallback;

public class FallbackStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new FallbackStrategyOptions();

        options.StrategyType.Should().Be("Fallback");
        options.Handler.Should().NotBeNull();
        options.Handler.IsEmpty.Should().BeTrue();
        options.OnFallback.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Validation()
    {
        var options = new FallbackStrategyOptions
        {
            OnFallback = null!,
            Handler = null!
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(o, "Invalid."))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid.

            Validation Errors:
            The ShouldHandle field is required.
            The OnFallback field is required.
            """);
    }
}
