using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class OnRetryArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnRetryArguments<int>(ResilienceContextPool.Shared.Get(), Outcome.FromResult(1), 2, TimeSpan.FromSeconds(3), TimeSpan.MaxValue);

        args.Context.Should().NotBeNull();
        args.Outcome.Result.Should().Be(1);
        args.AttemptNumber.Should().Be(2);
        args.RetryDelay.Should().Be(TimeSpan.FromSeconds(3));
        args.Duration.Should().Be(TimeSpan.MaxValue);
    }
}
