using System;
using System.Net.Http;
using Polly;
using Polly.Timeout;

namespace Snippets.Docs;

internal static class Timeout
{
    public static async Task Usage()
    {
        #region timeout

        // To add a default 30-second timeout
        new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions());

        // To add a timeout with a custom TimeSpan duration
        new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(3));

        // To add a timeout using a custom timeout generator function
        new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                TimeoutGenerator = args =>
                {
                    // Note: the timeout generator supports asynchronous operations
                    return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
                }
            });

        // To add a timeout and listen for timeout events
        new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                TimeoutGenerator = args =>
                {
                    // Note: the timeout generator supports asynchronous operations
                    return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
                },
                OnTimeout = args =>
                {
                    Console.WriteLine($"{args.Context.OperationKey}: Execution timed out after {args.Timeout.TotalSeconds} seconds.");
                    return default;
                }
            });

        #endregion

        var cancellationToken = CancellationToken.None;
        var httpClient = new HttpClient();
        var endpoint = new Uri("https://dummy");

        #region timeout-execution

        var pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(3))
            .Build();

        HttpResponseMessage httpResponse = await pipeline.ExecuteAsync(
              async ct =>
              {
                  // Execute a delegate that takes a CancellationToken as an input parameter.
                  return await httpClient.GetAsync(endpoint, ct);
              },
              cancellationToken);

        #endregion
    }
}
