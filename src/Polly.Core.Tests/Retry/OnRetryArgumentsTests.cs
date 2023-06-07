using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class OnRetryArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnRetryArguments(2, TimeSpan.FromSeconds(3));

        args.Attempt.Should().Be(2);
        args.RetryDelay.Should().Be(TimeSpan.FromSeconds(3));
    }
}
