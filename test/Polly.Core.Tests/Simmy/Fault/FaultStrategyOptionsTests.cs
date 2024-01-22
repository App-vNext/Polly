using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Fault;
using Polly.Utils;

namespace Polly.Core.Tests.Simmy.Fault;

public class FaultStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new FaultStrategyOptions();
        sut.Randomizer.Should().NotBeNull();
        sut.Enabled.Should().BeFalse();
        sut.EnabledGenerator.Should().BeNull();
        sut.InjectionRate.Should().Be(ChaosStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.Should().BeNull();
        sut.OnFaultInjected.Should().BeNull();
        sut.FaultGenerator.Should().BeNull();
    }

    [Fact]
    public void InvalidOptions()
    {
        var options = new FaultStrategyOptions
        {
            FaultGenerator = null!,
        };

        options.Invoking(o => ValidationHelper.ValidateObject(new(o, "Invalid Options")))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid Options

            Validation Errors:
            The FaultGenerator field is required.
            """);
    }
}
