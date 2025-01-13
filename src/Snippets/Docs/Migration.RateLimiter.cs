using System.Net.Http;
using System.Threading.RateLimiting;

namespace Snippets.Docs;

internal static partial class Migration
{
    public static void RateLimiter_V7()
    {
        #region migration-rate-limit-v7

        // Create sync rate limiter
        ISyncPolicy syncPolicy = Policy.RateLimit(
            numberOfExecutions: 100,
            perTimeSpan: TimeSpan.FromMinutes(1));

        // Create async rate limiter
        IAsyncPolicy asyncPolicy = Policy.RateLimitAsync(
            numberOfExecutions: 100,
            perTimeSpan: TimeSpan.FromMinutes(1));

        // Create generic sync rate limiter
        ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy.RateLimit<HttpResponseMessage>(
            numberOfExecutions: 100,
            perTimeSpan: TimeSpan.FromMinutes(1));

        // Create generic async rate limiter
        IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy.RateLimitAsync<HttpResponseMessage>(
            numberOfExecutions: 100,
            perTimeSpan: TimeSpan.FromMinutes(1));

        #endregion
    }

    public static void RateLimiter_V8()
    {
        #region migration-rate-limit-v8

        // The equivalent to Polly v7's RateLimit is the SlidingWindowRateLimiter.
        //
        // Polly exposes just a simple wrapper to the APIs exposed by the System.Threading.RateLimiting APIs.
        // There is no need to create separate instances for sync and async flows as ResiliencePipeline handles both scenarios.
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                SegmentsPerWindow = 4,
                Window = TimeSpan.FromMinutes(1),
            }))
            .Build();

        // The creation of generic pipeline is almost identical.
        //
        // Polly exposes the same set of rate-limiter extensions for both ResiliencePipeline<HttpResponseMessage> and ResiliencePipeline.
        ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                SegmentsPerWindow = 4,
                Window = TimeSpan.FromMinutes(1),
            }))
            .Build();

        #endregion
    }
}
