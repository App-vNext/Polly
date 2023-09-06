using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
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

        #endregion

        #region rate-limiter-partitioned

        var partitionedLimiter = PartitionedRateLimiter.Create<ResilienceContext, string>(context =>
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
                // PProvide a custom rate limiter delegate.
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

    public static async Task Disposal()
    {
        var services = new ServiceCollection();

        #region rate-limiter-disposal

        services
            .AddResiliencePipeline("my-pipeline", (builder, context) =>
            {
                var options = context.GetOptions<ConcurrencyLimiterOptions>("my-concurrency-options");

                // This call enables dynamic reloading of the pipeline
                // when the named ConcurrencyLimiterOptions change.
                context.EnableReloads<ConcurrencyLimiterOptions>("my-concurrency-options");

                var limiter = new ConcurrencyLimiter(options);

                builder.AddRateLimiter(limiter);

                // Dispose of the limiter when the pipeline is disposed.
                context.OnPipelineDisposed(() => limiter.Dispose());
            });

        #endregion
    }

    private static ValueTask<string> TextSearchAsync(string query, CancellationToken token) => new("dummy");

    private static string GetPartitionKey(ResilienceContext context) => string.Empty;
}
