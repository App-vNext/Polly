using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitBreakArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var context = ResilienceContext.Get();

        var args = new OnCircuitBreakArguments(context, TimeSpan.FromSeconds(2));

        args.Context.Should().Be(context);
        args.BreakDuration.Should().Be(TimeSpan.FromSeconds(2));
    }
}
