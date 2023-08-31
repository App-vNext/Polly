using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class RetryDelayGeneratorArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new RetryDelayGeneratorArguments<int>(ResilienceContextPool.Shared.Get(), Outcome.FromResult(1), 2);

        args.Context.Should().NotBeNull();
        args.Outcome.Result.Should().Be(1);
        args.AttemptNumber.Should().Be(2);
    }
}
