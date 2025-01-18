using Polly.Simmy.Fault;

namespace Polly.Core.Tests.Simmy.Fault;

public class FaultGeneratorTests
{
    [Fact]
    public void AddException_Generic_Ok()
    {
        var generator = new FaultGenerator();

        generator.AddException<InvalidOperationException>();

        Generate(generator).ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public void AddException_Factory_Ok()
    {
        var generator = new FaultGenerator();

        generator.AddException(() => new InvalidOperationException());

        Generate(generator).ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public void AddException_FactoryWithResilienceContext_Ok()
    {
        var generator = new FaultGenerator();

        generator.AddException(context =>
        {
            context.ShouldNotBeNull();

            return new InvalidOperationException();
        });

        Generate(generator).ShouldBeOfType<InvalidOperationException>();
    }

    private static Exception? Generate(FaultGenerator generator)
    {
        Func<FaultGeneratorArguments, ValueTask<Exception?>> func = generator;

        return func(
            new FaultGeneratorArguments(
                ResilienceContextPool.Shared.Get())).AsTask().Result;
    }
}
