using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerPredicateArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new CircuitBreakerPredicateArguments<int>(ResilienceContextPool.Shared.Get(), Outcome.FromResult(1));

        args.Context.Should().NotBeNull();
        args.Outcome.Result.Should().Be(1);
    }
}
