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
        args.Context.Should().Be(context);
        args.Outcome.Result.Should().Be(1);
        args.AttemptNumber.Should().Be(2);
    }
}
