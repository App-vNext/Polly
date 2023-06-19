using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class ShouldRetryArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new RetryPredicateArguments(2);
        args.Attempt.Should().Be(2);
    }
}
