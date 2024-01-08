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
                { Exception: HttpRequestException } => PredicateResult.True(),
                { Exception: TimeoutRejectedException } => PredicateResult.True(), // You can handle multiple exceptions
                { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
                _ => PredicateResult.False()
            }
        };

        #endregion
    }

    public static void ShouldHandleManualAsync()
    {
        #region should-handle-manual-async

        var options = new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = async args =>
            {
                if (args.Outcome.Exception is not null)
                {
                    return args.Outcome.Exception switch
                    {
                        HttpRequestException => true,
                        TimeoutRejectedException => true,
                        _ => false
                    };
                }

                // Determine whether to retry asynchronously or not based on the result.
                return await ShouldRetryAsync(args.Outcome.Result!, args.Context.CancellationToken);
            }
        };

        #endregion
    }

    private static Task<bool> ShouldRetryAsync(HttpResponseMessage response, CancellationToken cancellationToken) => Task.FromResult(true);

    public static void ShouldHandlePredicateBuilder()
    {
        #region should-handle-predicate-builder

        // Use PredicateBuilder<HttpResponseMessage> to simplify the setup of the ShouldHandle predicate.
        var options = new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .HandleResult(response => !response.IsSuccessStatusCode) // Handle results
                .Handle<HttpRequestException>() // Or handle exception
                .Handle<TimeoutRejectedException>() // Chaining is supported
        };

        #endregion
    }
}
