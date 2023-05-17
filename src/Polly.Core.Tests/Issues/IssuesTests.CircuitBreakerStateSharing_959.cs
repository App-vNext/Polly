using Polly.CircuitBreaker;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void CircuitBreakerStateSharing_959()
    {
        var options = new AdvancedCircuitBreakerStrategyOptions
        {
            FailureThreshold = 1,
            MinimumThroughput = 10
        };

        // handle int results
        options.ShouldHandle.HandleResult(-1);

        // handle string results
        options.ShouldHandle.HandleResult("error");

        // create the strategy
        var strategy = new ResilienceStrategyBuilder { TimeProvider = TimeProvider.Object }.AddAdvancedCircuitBreaker(options).Build();

        // now trigger the circuit breaker by evaluating multiple result types
        for (int i = 0; i < 5; i++)
        {
            strategy.Execute(_ => -1);
            strategy.Execute(_ => "error");
        }

        // now the circuit breaker should be open
        strategy.Invoking(s => s.Execute(_ => 0)).Should().Throw<BrokenCircuitException>();
        strategy.Invoking(s => s.Execute(_ => "valid-result")).Should().Throw<BrokenCircuitException>();

        // now wait for recovery
        TimeProvider.AdvanceTime(options.BreakDuration);

        // OK, circuit is closed now
        strategy.Execute(_ => 0);
        strategy.Execute(_ => "valid-result");
    }
}
