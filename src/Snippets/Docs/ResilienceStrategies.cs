using System.Net.Http;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace Snippets.Docs;

internal static class ResilienceStrategies
{
    public static async Task ResilienceStrategySample()
    {
        #region resilience-strategy-sample

        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(5)
            })
            .Build();

        #endregion
    }

    public static void ShouldHandle()
    {
        #region should-handle

        // Create an instance of options for a retry strategy. In this example,
        // we use RetryStrategyOptions. You could also use other options like
        // CircuitBreakerStrategyOptions or FallbackStrategyOptions.
        var options = new RetryStrategyOptions<HttpResponseMessage>();

        // PredicateBuilder can simplify the setup of the ShouldHandle predicate.
        options.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .HandleResult(response => !response.IsSuccessStatusCode)
            .Handle<HttpRequestException>();

        // For greater flexibility, you can directly use the ShouldHandle delegate with switch expressions.
        options.ShouldHandle = args => args.Outcome switch
        {
            // Strategies may offer additional context for result handling.
            // For instance, the retry strategy exposes the number of attempts made.
            _ when args.AttemptNumber > 3 => PredicateResult.False(),
            { Exception: HttpRequestException } => PredicateResult.True(),
            { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
            _ => PredicateResult.False()
        };

        #endregion
    }
}
