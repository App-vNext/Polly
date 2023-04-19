using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitOpenedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var context = ResilienceContext.Get();

        var args = new OnCircuitOpenedArguments(context, TimeSpan.FromSeconds(2));

        args.Context.Should().Be(context);
        args.BreakDuration.Should().Be(TimeSpan.FromSeconds(2));
    }
}
