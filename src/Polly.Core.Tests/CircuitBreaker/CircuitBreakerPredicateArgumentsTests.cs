using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerPredicateArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var context = ResilienceContext.Get();

        var args = new CircuitBreakerPredicateArguments(context);

        args.Context.Should().Be(context);
    }
}
