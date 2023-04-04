using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class OnTimeoutEventTests
{
    [Fact]
    public void Ctor_Ok()
    {
        this.Invoking(_ => new OnTimeoutEvent()).Should().NotThrow();
    }
}
