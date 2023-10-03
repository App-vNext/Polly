using System.Net.Http;
using Polly.Retry;
using Polly.Timeout;

namespace Snippets.Docs;

internal static class ResilienceStrategies
{
    public static async Task Usage()
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

    public static void ShouldHandleManual()
    {
        #region should-handle-manual

        var options = new RetryStrategyOptions<HttpResponseMessage>
        {
            // For greater flexibility, you can directly use the ShouldHandle delegate with switch expressions.
            ShouldHandle = args => args.Outcome switch
            {
                // Strategies may offer rich arguments for result handling.
                // For instance, the retry strategy exposes the number of attempts made.
                _ when args.AttemptNumber > 3 => PredicateResult.False(),
                { Exception: HttpRequestException } => PredicateResult.True(),
                { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
                _ => PredicateResult.False()
            }
        };

        #endregion
    }

    public static void ShouldHandlePredicateBuilder()
    {
        #region should-handle-predicate-builder

        // Use PredicateBuilder<HttpResponseMessage> to simplify the setup of the ShouldHandle predicate.
        var options = new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .HandleResult(response => !response.IsSuccessStatusCode) // Handle results
                .Handle<HttpRequestException>() // Or handle exceptions, chaining is supported
        };

        #endregion
    }
}
