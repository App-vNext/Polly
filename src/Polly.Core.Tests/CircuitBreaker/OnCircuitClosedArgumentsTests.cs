using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitClosedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var context = ResilienceContext.Get();

        var args = new OnCircuitClosedArguments(context);

        args.Context.Should().Be(context);
    }
}
