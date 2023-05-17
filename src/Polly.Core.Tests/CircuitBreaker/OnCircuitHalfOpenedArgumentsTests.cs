using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitHalfOpenedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var context = ResilienceContext.Get();

        var args = new OnCircuitHalfOpenedArguments(context);

        args.Context.Should().Be(context);
    }
}
