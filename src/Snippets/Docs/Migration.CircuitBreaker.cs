using System.Net.Http;
using Polly.CircuitBreaker;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static partial class Migration
{
    public static void CircuitBreaker_V7()
    {
        #region migration-circuit-breaker-v7

        // Create sync circuit breaker
        ISyncPolicy syncPolicy = Policy
            .Handle<SomeExceptionType>()
            .CircuitBreaker(
                exceptionsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(1));

        // Create async circuit breaker
        IAsyncPolicy asyncPolicy = Policy
            .Handle<SomeExceptionType>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(1));

        // Create generic sync circuit breaker
        ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy<HttpResponseMessage>
            .Handle<SomeExceptionType>()
            .CircuitBreaker(
                handledEventsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(1));

        // Create generic async circuit breaker
        IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy<HttpResponseMessage>
            .Handle<SomeExceptionType>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(1));

        #endregion
    }

    public static void AdvancedCircuitBreaker_V7()
    {
        #region migration-advanced-circuit-breaker-v7

        // Create sync advanced circuit breaker
        ISyncPolicy syncPolicy = Policy
            .Handle<SomeExceptionType>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5d,
                samplingDuration: TimeSpan.FromSeconds(5),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(1));

        // Create async advanced circuit breaker
        IAsyncPolicy asyncPolicy = Policy
            .Handle<SomeExceptionType>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5d,
                samplingDuration: TimeSpan.FromSeconds(5),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(1));

        // Create generic sync advanced circuit breaker
        ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy<HttpResponseMessage>
            .Handle<SomeExceptionType>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5d,
                samplingDuration: TimeSpan.FromSeconds(5),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(1));

        // Create generic async advanced circuit breaker
        IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy<HttpResponseMessage>
            .Handle<SomeExceptionType>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5d,
                samplingDuration: TimeSpan.FromSeconds(5),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(1));

        // Check circuit state
        ICircuitBreakerPolicy cbPolicy = (ICircuitBreakerPolicy)asyncPolicy;
        bool isOpen = cbPolicy.CircuitState == CircuitState.Open || cbPolicy.CircuitState == CircuitState.Isolated;

        // Manually control state
        cbPolicy.Isolate(); // Transitions into the Isolated state
        cbPolicy.Reset(); // Transitions into the Closed state

        #endregion
    }

    public static async Task CircuitBreaker_V8()
    {
        #region migration-circuit-breaker-v8

        // Create pipeline with circuit breaker. Because ResiliencePipeline supports both sync and async
        // callbacks, there is no need to define it twice.
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
                FailureRatio = 0.5d,
                SamplingDuration = TimeSpan.FromSeconds(5),
                MinimumThroughput = 2,
                BreakDuration = TimeSpan.FromSeconds(1)
            })
            .Build();

        // Create a generic pipeline with circuit breaker. Because ResiliencePipeline<T> supports both sync and async
        // callbacks, there is also no need to define it twice.
        ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<SomeExceptionType>(),
                FailureRatio = 0.5d,
                SamplingDuration = TimeSpan.FromSeconds(5),
                MinimumThroughput = 2,
                BreakDuration = TimeSpan.FromSeconds(1)
            })
            .Build();

        // Check circuit state
        CircuitBreakerStateProvider stateProvider = new();
        // Manually control state
        CircuitBreakerManualControl manualControl = new();

        ResiliencePipeline pipelineState = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
                FailureRatio = 0.5d,
                SamplingDuration = TimeSpan.FromSeconds(5),
                MinimumThroughput = 2,
                BreakDuration = TimeSpan.FromSeconds(1),
                StateProvider = stateProvider,
                ManualControl = manualControl
            })
            .Build();

        // Check circuit state
        bool isOpen = stateProvider.CircuitState == CircuitState.Open || stateProvider.CircuitState == CircuitState.Isolated;

        // Manually control state
        await manualControl.IsolateAsync(); // Transitions into the Isolated state
        await manualControl.CloseAsync(); // Transitions into the Closed state

        #endregion
    }
}
