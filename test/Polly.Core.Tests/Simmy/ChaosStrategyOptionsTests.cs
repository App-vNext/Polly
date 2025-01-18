using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Utils;

namespace Polly.Core.Tests.Simmy;
public class ChaosStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new TestChaosStrategyOptions();

        sut.Randomizer.ShouldNotBeNull();
        sut.Enabled.ShouldBeTrue();
        sut.EnabledGenerator.ShouldBeNull();
        sut.InjectionRate.ShouldBe(ChaosStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.ShouldBeNull();
    }

    [InlineData(-1)]
    [InlineData(1.1)]
    [Theory]
    public void InvalidThreshold(double injectionRate)
    {
        var sut = new TestChaosStrategyOptions
        {
            InjectionRate = injectionRate,
        };

        var exception = Should.Throw<ValidationException>(() => ValidationHelper.ValidateObject(new(sut, "Invalid Options")));
        exception.Message.Trim().ShouldBe("""
            Invalid Options
            Validation Errors:
            The field InjectionRate must be between 0 and 1.
            """,
            StringCompareShould.IgnoreLineEndings);
    }
}
