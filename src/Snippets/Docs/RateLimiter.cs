using System.Threading.RateLimiting;
using Polly;
using Polly.RateLimiting;

namespace Snippets.Docs;

internal static class RateLimiter
{
    public static void Usage()
    {
        #region rate-limiter

        // Add rate limiter with default options.
        // See https://github.com/App-vNext/Polly/blob/main/docs/strategies/rate-limiter.md#defaults for default values.
        new ResiliencePipelineBuilder()
            .AddRateLimiter(new RateLimiterStrategyOptions());

        // Create a rate limiter to allow a maximum of 100 concurrent executions and a queue of 50.
        new ResiliencePipelineBuilder()
            .AddConcurrencyLimiter(100, 50);

        // Create a rate limiter that allows 100 executions per minute.
        new ResiliencePipelineBuilder()
            .AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

        // Create a custom partitioned rate limiter.
        var partitionedLimiter = PartitionedRateLimiter.Create<Polly.ResilienceContext, string>(context =>
        {
            // Extract the partition key.
            string partitionKey = GetPartitionKey(context);

            return RateLimitPartition.GetConcurrencyLimiter(
                partitionKey,
                key => new ConcurrencyLimiterOptions
                {
                    PermitLimit = 100
                });
        });

        new ResiliencePipelineBuilder()
            .AddRateLimiter(new RateLimiterStrategyOptions
            {
                // Provide a custom rate limiter delegate.
                RateLimiter = args =>
                {
                    return partitionedLimiter.AcquireAsync(args.Context, 1, args.Context.CancellationToken);
                }
            });

        #endregion
    }

    public static async Task Execution()
    {
        var cancellationToken = CancellationToken.None;
        var query = "dummy";

        #region rate-limiter-execution

        var pipeline = new ResiliencePipelineBuilder().AddConcurrencyLimiter(100, 50).Build();

        try
        {
            // Execute an asynchronous text search operation.
            var result = await pipeline.ExecuteAsync(
                token => TextSearchAsync(query, token),
                cancellationToken);
        }
        catch (RateLimiterRejectedException ex)
        {
            // Handle RateLimiterRejectedException,
            // that can optionally contain information about when to retry.
            if (ex.RetryAfter is TimeSpan retryAfter)
            {
                Console.WriteLine($"Retry After: {retryAfter}");
            }
        }

        #endregion
    }

    private static ValueTask<string> TextSearchAsync(string query, CancellationToken token) => new("dummy");

    private static string GetPartitionKey(Polly.ResilienceContext context) => string.Empty;
}
