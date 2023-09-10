using System.Globalization;
using System.Net.Http;
using Polly;
using Polly.Retry;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static class Retry
{
    public static void Usage()
    {
        #region retry

        // Add retry using the default options.
        // See https://github.com/App-vNext/Polly/blob/main/docs/strategies/retry.md#defaults for default values.
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions());

        // For instant retries with no delay
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            Delay = TimeSpan.Zero
        });

        // For advanced control over the retry behavior, including the number of attempts,
        // delay between retries, and the types of exceptions to handle.
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,  // Adds a random factor to the delay
            MaxRetryAttempts = 4,
            Delay = TimeSpan.FromSeconds(3),
        });

        // To use a custom function to generate the delay for retries
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            DelayGenerator = args =>
            {
                var delay = args.AttemptNumber switch
                {
                    0 => TimeSpan.Zero,
                    1 => TimeSpan.FromSeconds(1),
                    _ => TimeSpan.FromSeconds(5)
                };

                // This example uses a synchronous delay generator,
                // but the API also supports asynchronous implementations.
                return new ValueTask<TimeSpan?>(delay);
            }
        });

        // To extract the delay from the result object
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            DelayGenerator = args =>
            {
                if (args.Outcome.Result is HttpResponseMessage responseMessage &&
                    TryGetDelay(responseMessage, out TimeSpan delay))
                {
                    return new ValueTask<TimeSpan?>(delay);
                }

                // Returning null means the retry strategy will use its internal delay for this attempt.
                return new ValueTask<TimeSpan?>((TimeSpan?)null);
            }
        });

        // To get notifications when a retry is performed
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            OnRetry = args =>
            {
                Console.WriteLine("OnRetry, Attempt: {0}", args.AttemptNumber);

                // Event handlers can be asynchronous; here, we return an empty ValueTask.
                return default;
            }
        });

        // To keep retrying indefinitely or until success use int.MaxValue.
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = int.MaxValue,
        });

        #endregion
    }

    private static bool TryGetDelay(HttpResponseMessage response, out TimeSpan delay)
    {
        if (response.Headers.TryGetValues("Retry-After", out var values) &&
            values.FirstOrDefault() is string retryAfterValue &&
            int.TryParse(retryAfterValue, CultureInfo.InvariantCulture, out int retryAfterSeconds))
        {
            delay = TimeSpan.FromSeconds(retryAfterSeconds);
            return true;
        }

        delay = TimeSpan.Zero;
        return false;
    }
}
