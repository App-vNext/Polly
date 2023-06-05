using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitOpenedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnCircuitOpenedArguments(TimeSpan.FromSeconds(2), true);

        args.BreakDuration.Should().Be(TimeSpan.FromSeconds(2));
        args.IsManual.Should().BeTrue();
    }
}
