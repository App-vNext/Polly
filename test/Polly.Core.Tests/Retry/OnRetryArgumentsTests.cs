using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public static class OnRetryArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        // Act
        var args = new OnRetryArguments<int>(context, Outcome.FromResult(1), 2, TimeSpan.FromSeconds(3), TimeSpan.MaxValue);

        // Assert
        args.Context.ShouldBe(context);
        args.Outcome.Result.ShouldBe(1);
        args.AttemptNumber.ShouldBe(2);
        args.RetryDelay.ShouldBe(TimeSpan.FromSeconds(3));
        args.Duration.ShouldBe(TimeSpan.MaxValue);
    }
}
