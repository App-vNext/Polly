using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitClosedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnCircuitClosedArguments(true);
        args.IsManual.Should().BeTrue();
    }
}
