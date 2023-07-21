using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerPredicateArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        this.Invoking(_ => default(CircuitBreakerPredicateArguments)).Should().NotThrow();
    }
}
