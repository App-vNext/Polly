using System.Net.Http;
using Polly.Timeout;

namespace Snippets.Docs;

internal static class Timeout
{
    public static async Task Usage()
    {
        #region timeout

        // To add a timeout with a custom TimeSpan duration
        new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromSeconds(3));

        // Timeout using the default options.
        // See https://www.pollydocs.org/strategies/timeout#defaults for defaults.
        var optionsDefaults = new TimeoutStrategyOptions();

        // To add a timeout using a custom timeout generator function
        var optionsTimeoutGenerator = new TimeoutStrategyOptions
        {
            TimeoutGenerator = static args =>
            {
                // Note: the timeout generator supports asynchronous operations
                return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
            }
        };

        // To add a timeout and listen for timeout events
        var optionsOnTimeout = new TimeoutStrategyOptions
        {
            TimeoutGenerator = static args =>
            {
                // Note: the timeout generator supports asynchronous operations
                return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(123));
            },
            OnTimeout = static args =>
            {
                Console.WriteLine($"{args.Context.OperationKey}: Execution timed out after {args.Timeout.TotalSeconds} seconds.");
                return default;
            }
        };

        // Add a timeout strategy with a TimeoutStrategyOptions instance to the pipeline
        new ResiliencePipelineBuilder().AddTimeout(optionsDefaults);

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

    public static async Task IgnoreCancellationToken()
    {
        var outerToken = CancellationToken.None;

        #region timeout-ignore-cancellation-token

        var pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        await pipeline.ExecuteAsync(
            async innerToken => await Task.Delay(TimeSpan.FromSeconds(3), outerToken), // The delay call should use innerToken
            outerToken);

        #endregion
    }

    public static async Task RespectCancellationToken()
    {
        var outerToken = CancellationToken.None;

        #region timeout-respect-cancellation-token

        var pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        await pipeline.ExecuteAsync(
            static async innerToken => await Task.Delay(TimeSpan.FromSeconds(3), innerToken),
            outerToken);

        #endregion
    }
}
