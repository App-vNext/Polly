﻿using System.ComponentModel.DataAnnotations;
using Polly.Utils;

namespace Polly.Core.Tests.Simmy;
public class MonkeyStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new TestChaosStrategyOptions();

        sut.StrategyType.Should().Be("Test");

        sut.Randomizer.Should().NotBeNull();

        sut.Enabled.Should().BeNull();
        sut.EnabledGenerator.Should().BeNull();

        sut.InjectionRate.Should().BeNull();
        sut.InjectionRateGenerator.Should().BeNull();
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

        sut
            .Invoking(o => ValidationHelper.ValidateObject(o, "Invalid Options"))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid Options
            
            Validation Errors:
            The field InjectionRate must be between 0 and 1.
            """);
    }
}
