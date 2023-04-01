using Polly.Retry;

namespace Polly.Core.Tests.Retry;
public class ShouldRetryArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new ShouldRetryArguments(ResilienceContext.Get(), 2, TimeSpan.FromSeconds(2));

        args.Context.Should().NotBeNull();
        args.AttemptNumber.Should().Be(2);
        args.TotalExecutionTime.Should().Be(TimeSpan.FromSeconds(2));
    }
}
