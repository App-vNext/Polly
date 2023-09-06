using System.Net;
using System.Net.Http;
using Polly;
using Polly.Hedging;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static class Hedging
{
    public static void Usage()
    {
        #region hedging

        // Add hedging with default options.
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>());

        // Add a customized hedging strategy that retries up to 3 times if the execution
        // takes longer than 1 second or if it fails due to an exception or returns an HTTP 500 Internal Server Error.
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<SomeExceptionType>()
                    .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError),
                MaxHedgedAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                ActionGenerator = args =>
                {
                    Console.WriteLine("Preparing to execute hedged action.");

                    // Return a delegate function to invoke the original action with the action context.
                    // Optionally, you can also create a completely new action to be executed.
                    return () => args.Callback(args.ActionContext);
                }
            });

        // Subscribe to hedging events.
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
            {
                OnHedging = args =>
                {
                    Console.WriteLine($"OnHedging: Attempt number {args.AttemptNumber}");
                    return default;
                }
            });

        #endregion
    }
}
