using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Behavior;
using Polly.Utils;

namespace Polly.Core.Tests.Simmy.Behavior;

public class ChaosBehaviorStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new ChaosBehaviorStrategyOptions();
        sut.Randomizer.ShouldNotBeNull();
        sut.Enabled.ShouldBeTrue();
        sut.EnabledGenerator.ShouldBeNull();
        sut.InjectionRate.ShouldBe(ChaosStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.ShouldBeNull();
        sut.BehaviorGenerator.ShouldBeNull();
        sut.OnBehaviorInjected.ShouldBeNull();
    }

    [Fact]
    public void InvalidOptions()
    {
        var sut = new ChaosBehaviorStrategyOptions();

        var exception = Should.Throw<ValidationException>(() => ValidationHelper.ValidateObject(new(sut, "Invalid Options")));
        exception.Message.Trim().ShouldBe("""
            Invalid Options
            Validation Errors:
            The BehaviorGenerator field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }
}
