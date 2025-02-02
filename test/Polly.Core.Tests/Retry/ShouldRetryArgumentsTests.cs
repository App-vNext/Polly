using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public static class ShouldRetryArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new RetryPredicateArguments<int>(context, Outcome.FromResult(1), 2);

        // Assert
        args.Context.ShouldBe(context);
        args.Outcome.Result.ShouldBe(1);
        args.AttemptNumber.ShouldBe(2);
    }
}
