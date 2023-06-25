namespace Polly.Core.Tests;

public class OutcomeArgumentsTests
{
    [Fact]
    public void Ctor_Result_Ok()
    {
        var args = new OutcomeArguments<string, string>(
            ResilienceContext.Get(),
            Outcome.FromResult("dummy"),
            "args");

        args.Context.Should().NotBeNull();
        args.Exception.Should().BeNull();
        args.Outcome.Result.Should().Be("dummy");
        args.Arguments.Should().Be("args");
        args.Result.Should().Be("dummy");
    }

    [Fact]
    public void Ctor_Exception_Ok()
    {
        var args = new OutcomeArguments<string, string>(
            ResilienceContext.Get(),
            Outcome.FromException<string>(new InvalidOperationException()),
            "args");

        args.Context.Should().NotBeNull();
        args.Exception.Should().BeOfType<InvalidOperationException>();
        args.Arguments.Should().Be("args");
        args.Result.Should().BeNull();
    }
}
