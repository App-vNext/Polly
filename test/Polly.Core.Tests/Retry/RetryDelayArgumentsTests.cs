using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class RetryDelayArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new RetryDelayArguments<int>(ResilienceContextPool.Shared.Get(), Outcome.FromResult(1), 2, TimeSpan.FromSeconds(2));

        args.Context.Should().NotBeNull();
        args.Outcome.Result.Should().Be(1);
        args.AttemptNumber.Should().Be(2);
        args.DelayHint.Should().Be(TimeSpan.FromSeconds(2));
    }
}
