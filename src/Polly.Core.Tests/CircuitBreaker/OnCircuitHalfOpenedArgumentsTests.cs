using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitHalfOpenedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        this.Invoking(_ => new OnCircuitHalfOpenedArguments()).Should().NotThrow();
    }
}
