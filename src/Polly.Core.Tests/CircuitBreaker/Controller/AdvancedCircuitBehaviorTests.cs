using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker.Controller;
public class AdvancedCircuitBehaviorTests
{
    [Fact]
    public void HappyPath()
    {
        var behavior = new AdvancedCircuitBehavior();

        behavior
            .Invoking(b =>
            {
                behavior.OnActionFailure(CircuitState.Closed, out var shouldBreak);
                shouldBreak.Should().BeFalse();
                behavior.OnCircuitClosed();
                behavior.OnActionSuccess(CircuitState.Closed);
            })
            .Should()
            .NotThrow();
    }
}
