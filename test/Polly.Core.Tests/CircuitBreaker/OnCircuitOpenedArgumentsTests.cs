using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitOpenedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnCircuitOpenedArguments<int>(ResilienceContextPool.Shared.Get(), Outcome.FromResult(1), TimeSpan.FromSeconds(2), true);

        args.Context.Should().NotBeNull();
        args.Outcome.Result.Should().Be(1);
        args.BreakDuration.Should().Be(TimeSpan.FromSeconds(2));
        args.IsManual.Should().BeTrue();
    }
}
