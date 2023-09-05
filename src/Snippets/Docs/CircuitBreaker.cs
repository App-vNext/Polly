using System.Net;
using System.Net.Http;
using Polly;
using Polly.CircuitBreaker;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

#pragma warning disable S103 // Lines should not be too long

internal static class CircuitBreaker
{
    public static async Task CircuitBreakerExample()
    {
        #region circuit-breaker

        // Use the default Circuit Breaker options with pre-configured settings:
        //
        // FailureRatio: 0.1 — Opens the circuit if 10% of the calls fail
        // BreakDuration: 30 seconds — Duration the circuit stays open before another try
        // MinimumThroughput: 100 — Minimum number of calls required before the circuit can break
        // SamplingDuration: 30 seconds — Time window for monitoring calls before allowing the circuit to break
        // ShouldHandle: Handles all exceptions except for OperationCanceledException
        new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions());

        // Configure custom settings for the Circuit Breaker:
        //
        // The circuit will break if more than 50% of actions result in handled exceptions,
        // within any 10-second sampling duration, and at least 8 actions are processed.
        new ResiliencePipelineBuilder().AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8,
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>()
        });

        // Handle specific failed results for HttpResponseMessage:
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<SomeExceptionType>()
                    .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
            });

        // Monitor the circuit state, useful for health reporting:
        var stateProvider = new CircuitBreakerStateProvider();

        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddCircuitBreaker(new()
            {
                StateProvider = stateProvider
            })
            .Build();

        /*
        CircuitState.Closed - Normal operation; actions are executed.
        CircuitState.Open - Circuit is open; actions are blocked.
        CircuitState.HalfOpen - Recovery state after break duration expires; actions are permitted.
        CircuitState.Isolated - Circuit is manually held open; actions are blocked.
        */

        // Manually control the Circuit Breaker state:
        var manualControl = new CircuitBreakerManualControl();

        new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new()
            {
                ManualControl = manualControl
            })
            .Build();

        // Manually isolate a circuit, e.g., to isolate a downstream service.
        await manualControl.IsolateAsync();

        // Manually close the circuit to accept actions again.
        await manualControl.CloseAsync();

        #endregion
    }
}
