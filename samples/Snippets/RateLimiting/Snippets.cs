using Polly;
using Polly.RateLimiting;
using System.Threading.RateLimiting;

namespace Snippets.RateLimiting;

internal static class Snippets
{
    public static void Usage()
    {
        #region rate-limiter-usage

        ResiliencePipelineBuilder builder = new ResiliencePipelineBuilder();

        // Convenience extension method for ConcurrencyLimiter
        builder.AddConcurrencyLimiter(permitLimit: 10, queueLimit: 10);

        // Convenience extension method for ConcurrencyLimiter with callback
        builder.AddConcurrencyLimiter(
            new ConcurrencyLimiterOptions
            {
                PermitLimit = 10,
                QueueLimit = 10
            });

        // Convenience extension method with custom limiter creation
        builder.AddRateLimiter(
            new ConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = 10,
                QueueLimit = 10
            }));

        // Add rate limiter using the RateLimiterStrategyOptions
        var limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = 10,
            QueueLimit = 10
        });

        builder.AddRateLimiter(new RateLimiterStrategyOptions
        {
            RateLimiter = args => limiter.AcquireAsync(1, args.Context.CancellationToken),
            OnRejected = _ =>
            {
                Console.WriteLine("Rate limiter rejected!");
                return default;
            }
        });

        #endregion
    }
}
