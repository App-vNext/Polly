using Polly.Retry;

namespace Polly.Core.Tests.Strategy;

public class OnRetryArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnRetryArguments(ResilienceContext.Get(), 2);

        args.Context.Should().NotBeNull();
        args.Attempt.Should().Be(2);
    }
}
