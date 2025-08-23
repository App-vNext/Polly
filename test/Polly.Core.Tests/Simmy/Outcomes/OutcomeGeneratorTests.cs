using Polly.Simmy.Outcomes;

namespace Polly.Core.Tests.Simmy.Outcomes;

public class OutcomeGeneratorTests
{
    [Fact]
    public void AddException_Generic_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddException<InvalidOperationException>();

        Generate(generator)!.Value.Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public void AddException_Factory_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddException(() => new InvalidOperationException());

        Generate(generator)!.Value.Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public void AddException_FactoryWithResilienceContext_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddException(context =>
        {
            context.ShouldNotBeNull();

            return new InvalidOperationException();
        });

        Generate(generator)!.Value.Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public void AddResult_Factory_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddResult(() =>
        {
            return "dummy";
        });

        Generate(generator)!.Value.Result.ShouldBe("dummy");
    }

    [Fact]
    public void AddResult_FactoryWithResilienceContext_Ok()
    {
        var generator = new OutcomeGenerator<string>();

        generator.AddResult(context =>
        {
            context.ShouldNotBeNull();

            return "dummy";
        });

        Generate(generator)!.Value.Result.ShouldBe("dummy");
    }

    private static Outcome<string>? Generate(OutcomeGenerator<string> generator)
    {
        Func<OutcomeGeneratorArguments, ValueTask<Outcome<string>?>> func = generator;

        return func(
            new OutcomeGeneratorArguments(
                ResilienceContextPool.Shared.Get(TestCancellation.Token))).AsTask().Result;
    }
}
