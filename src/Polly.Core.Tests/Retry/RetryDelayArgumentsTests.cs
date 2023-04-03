using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class RetryDelayArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new RetryDelayArguments(ResilienceContext.Get(), 2);

        args.Context.Should().NotBeNull();
        args.Attempt.Should().Be(2);
    }
}
