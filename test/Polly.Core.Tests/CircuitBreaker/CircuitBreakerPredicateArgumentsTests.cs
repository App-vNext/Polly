using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public static class CircuitBreakerPredicateArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        var args = new CircuitBreakerPredicateArguments<int>(
            ResilienceContextPool.Shared.Get(CancellationToken.None),
            Outcome.FromResult(1));

        args.Context.ShouldNotBeNull();
        args.Outcome.Result.ShouldBe(1);
    }
}
