using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitHalfOpenedArgumentsTests
{
    [Fact]
    public void Ctor_Ok() =>
        new OnCircuitHalfOpenedArguments(ResilienceContextPool.Shared.Get()).Context.Should().NotBeNull();
}
