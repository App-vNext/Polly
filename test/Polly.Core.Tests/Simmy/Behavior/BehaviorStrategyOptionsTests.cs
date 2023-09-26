﻿using System.ComponentModel.DataAnnotations;
using Polly.Simmy;
using Polly.Simmy.Behavior;
using Polly.Utils;

namespace Polly.Core.Tests.Simmy.Behavior;

public class BehaviorStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var sut = new BehaviorStrategyOptions();
        sut.Randomizer.Should().NotBeNull();
        sut.Enabled.Should().BeFalse();
        sut.EnabledGenerator.Should().BeNull();
        sut.InjectionRate.Should().Be(MonkeyStrategyConstants.DefaultInjectionRate);
        sut.InjectionRateGenerator.Should().BeNull();
        sut.BehaviorAction.Should().BeNull();
        sut.OnBehaviorInjected.Should().BeNull();
    }

    [Fact]
    public void InvalidOptions()
    {
        var sut = new BehaviorStrategyOptions();

        sut
            .Invoking(o => ValidationHelper.ValidateObject(new(o, "Invalid Options")))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid Options
            
            Validation Errors:
            The BehaviorAction field is required.
            """);
    }
}
