using Polly.Retry;

namespace Polly.Core.Tests.Retry;

public class RetryStrategyOptionsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var options = new RetryStrategyOptions();

        options.ShouldRetry.Should().NotBeNull();
        options.ShouldRetry.IsEmpty.Should().BeTrue();

        options.RetryDelayGenerator.Should().NotBeNull();
        options.RetryDelayGenerator.IsEmpty.Should().BeTrue();
    }
}
