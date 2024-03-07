﻿using System;
using Polly.Simmy.Utils;

namespace Polly.Core.Tests.Simmy.Utils;

public class GeneratorHelperTests
{
    [Fact]
    public void CreateGenerator_NoGenerators_ThrowsInvalidOperationException()
    {
        var helper = new GeneratorHelper<int>(_ => 10);

        Action act = () => helper.CreateGenerator();
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("No outcome generators have been added.");
    }

    [Fact]
    public void AddOutcome_EnsureWeightRespected()
    {
        int weight = 0;
        int maxWeight = 0;
        var context = ResilienceContextPool.Shared.Get();

        var helper = new GeneratorHelper<int>(max =>
        {
            maxWeight = max;
            return weight;
        });

        helper.AddOutcome(_ => Outcome.FromResult(1), 40);
        helper.AddOutcome(_ => Outcome.FromResult(2), 80);

        var generator = helper.CreateGenerator();

        weight = 0;
        generator(context).Result.Should().Be(1);
        weight = 39;
        generator(context).Result.Should().Be(1);

        weight = 40;
        generator(context).Result.Should().Be(2);

        maxWeight.Should().Be(120);
    }

    [Fact]
    public void Generator_OutsideRange_ReturnsOutcomeWithDefault()
    {
        var context = ResilienceContextPool.Shared.Get();
        var helper = new GeneratorHelper<int>(_ => 1000);

        helper.AddOutcome(_ => Outcome.FromResult(1), 40);

        var generator = helper.CreateGenerator();
        generator(context).Should().Be(Outcome.FromResult(default(int)));
    }
}
