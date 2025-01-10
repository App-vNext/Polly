using Polly.CircuitBreaker;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void CircuitBreakerStateSharing_959()
    {
        var cancellationToken = CancellationToken.None;
        var options = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 1,
            MinimumThroughput = 10,
            ShouldHandle = args => args.Outcome.Result switch
            {
                // handle int results
                int intVal when intVal == -1 => new ValueTask<bool>(true),

                // handle string results
                string stringVal when stringVal == "error" => new ValueTask<bool>(true),
                _ => new ValueTask<bool>(false),
            },
        };

        // create the strategy
        var strategy = new ResiliencePipelineBuilder { TimeProvider = TimeProvider }.AddCircuitBreaker(options).Build();

        // now trigger the circuit breaker by evaluating multiple result types
        for (int i = 0; i < 5; i++)
        {
            strategy.Execute(_ => -1, cancellationToken);
            strategy.Execute(_ => "error", cancellationToken);
        }

        // now the circuit breaker should be open
        strategy.Invoking(s => s.Execute(_ => 0)).Should().Throw<BrokenCircuitException>();
        strategy.Invoking(s => s.Execute(_ => "valid-result")).Should().Throw<BrokenCircuitException>();

        // now wait for recovery
        TimeProvider.Advance(options.BreakDuration);

        // OK, circuit is closed now
        strategy.Execute(_ => 0, cancellationToken);
        strategy.Execute(_ => "valid-result", cancellationToken);
    }
}
