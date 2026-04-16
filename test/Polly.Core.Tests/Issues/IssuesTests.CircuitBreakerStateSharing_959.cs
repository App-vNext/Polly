using Polly.CircuitBreaker;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void CircuitBreakerStateSharing_959()
    {
        var cancellationToken = TestCancellation.Token;
        var options = new CircuitBreakerStrategyOptions
        {
            FailureRatio = 1,
            MinimumThroughput = 10,
#if UNION_TYPES
            ShouldHandle = args => args.Outcome switch
            {
                object result when result is int intVal && intVal is -1 => new ValueTask<bool>(true),
                object result when result is string stringVal && stringVal is "error" => new ValueTask<bool>(true),
                _ => new ValueTask<bool>(false),
            },
#else
            ShouldHandle = args => args.Outcome.Result switch
            {
                // handle int results
                int intVal when intVal is -1 => new ValueTask<bool>(true),

                // handle string results
                string stringVal when stringVal is "error" => new ValueTask<bool>(true),
                _ => new ValueTask<bool>(false),
            },
#endif
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
        Should.Throw<BrokenCircuitException>(() => strategy.Execute(_ => 0));
        Should.Throw<BrokenCircuitException>(() => strategy.Execute(_ => "valid-result"));

        // now wait for recovery
        TimeProvider.Advance(options.BreakDuration);

        // OK, circuit is closed now
        strategy.Execute(_ => 0, cancellationToken);
        strategy.Execute(_ => "valid-result", cancellationToken);
    }
}
