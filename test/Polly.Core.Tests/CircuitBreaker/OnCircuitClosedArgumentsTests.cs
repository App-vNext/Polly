using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class OnCircuitClosedArgumentsTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var args = new OnCircuitClosedArguments<int>(ResilienceContextPool.Shared.Get(), Outcome.FromResult(1), true);

        args.Context.Should().NotBeNull();
        args.Outcome.Result.Should().Be(1);
        args.IsManual.Should().BeTrue();
    }
}
