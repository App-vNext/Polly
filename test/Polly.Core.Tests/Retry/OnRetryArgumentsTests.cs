using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public static class OnRetryArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();

        // Act
        var args = new OnRetryArguments<int>(context, Outcome.FromResult(1), 2, TimeSpan.FromSeconds(3), TimeSpan.MaxValue);

        // Assert
        args.Context.Should().Be(context);
        args.Outcome.Result.Should().Be(1);
        args.AttemptNumber.Should().Be(2);
        args.RetryDelay.Should().Be(TimeSpan.FromSeconds(3));
        args.Duration.Should().Be(TimeSpan.MaxValue);
    }
}
