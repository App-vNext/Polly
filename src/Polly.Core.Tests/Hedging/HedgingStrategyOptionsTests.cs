using System.ComponentModel.DataAnnotations;
using Polly.Hedging;
using Polly.Utils;

namespace Polly.Core.Tests.Hedging;

public class HedgingStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new HedgingStrategyOptions();

        options.StrategyType.Should().Be("Hedging");
        options.Handler.Should().NotBeNull();
        options.Handler.IsEmpty.Should().BeTrue();
        options.HedgingDelayGenerator.Should().BeNull();
        options.HedgingDelay.Should().Be(TimeSpan.FromSeconds(2));
        options.MaxHedgedAttempts.Should().Be(2);
        options.OnHedging.Should().BeNull();
    }

    [Fact]
    public void Validation()
    {
        var options = new HedgingStrategyOptions
        {
            HedgingDelayGenerator = null!,
            Handler = null!,
            MaxHedgedAttempts = -1,
            OnHedging = null!
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(o, "Invalid."))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid.

            Validation Errors:
            The field MaxHedgedAttempts must be between 2 and 10.
            The Handler field is required.
            """);
    }
}
