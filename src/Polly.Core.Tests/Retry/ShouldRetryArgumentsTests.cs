using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class ShouldRetryArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new ShouldRetryArguments(ResilienceContext.Get(), 2);

        args.Context.Should().NotBeNull();
        args.Attempt.Should().Be(2);
    }
}
