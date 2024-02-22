using System;
using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeGeneratorTests
{
    [Fact]
    public void AddException_Generic_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddException<InvalidOperationException>();

        Generate(generator).Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void AddException_Factory_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddException(() => new InvalidOperationException());

        Generate(generator).Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void AddException_FactoryWithResilienceContext_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddException(context =>
        {
            context.Should().NotBeNull();

            return new InvalidOperationException();
        });

        Generate(generator).Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void AddResult_Factory_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddResult(() =>
        {
            return "dummy";
        });

        Generate(generator).Result.Should().Be("dummy");
    }

    [Fact]
    public void AddResult_FactoryWithResilienceContext_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddResult(context =>
        {
            context.Should().NotBeNull();

            return "dummy";
        });

        Generate(generator).Result.Should().Be("dummy");
    }

    private static Outcome<string> Generate(OutcomeGenerator<string> generator)
    {
        Func<OutcomeGeneratorArguments, ValueTask<Outcome<string>>> func = generator;

        return func(new OutcomeGeneratorArguments(ResilienceContextPool.Shared.Get())).AsTask().Result;
    }
}
