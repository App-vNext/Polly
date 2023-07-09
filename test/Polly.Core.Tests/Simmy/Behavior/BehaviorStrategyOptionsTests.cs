using System.ComponentModel.DataAnnotations;
using Polly.Simmy.Behavior;
using Polly.Utils;

namespace Polly.Core.Tests.Simmy.Behavior;

public class BehaviorStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new BehaviorStrategyOptions();
        sut.StrategyType.Should().Be(BehaviorConstants.StrategyType);
        sut.Randomizer.Should().NotBeNull();
        sut.Enabled.Should().BeNull();
        sut.EnabledGenerator.Should().BeNull();
        sut.InjectionRate.Should().BeNull();
        sut.InjectionRateGenerator.Should().BeNull();
        sut.Behavior.Should().BeNull();
        sut.OnBehaviorInjected.Should().BeNull();
    }

    [Fact]
    public void InvalidOptions()
    {
        var sut = new BehaviorStrategyOptions();

        sut
            .Invoking(o => ValidationHelper.ValidateObject(o, "Invalid Options"))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid Options
            
            Validation Errors:
            The Behavior field is required.
            """);
    }
}
