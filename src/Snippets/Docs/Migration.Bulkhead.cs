using System.Net.Http;

namespace Snippets.Docs;

internal static partial class Migration
{
    public static void Bulkhead_V7()
    {
        #region migration-bulkhead-v7

        // Create sync bulkhead
        ISyncPolicy syncPolicy = Policy.Bulkhead(maxParallelization: 100, maxQueuingActions: 50);

        // Create async bulkhead
        IAsyncPolicy asyncPolicy = Policy.BulkheadAsync(maxParallelization: 100, maxQueuingActions: 50);

        // Create generic sync bulkhead
        ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy.Bulkhead<HttpResponseMessage>(maxParallelization: 100, maxQueuingActions: 50);

        // Create generic async bulkhead
        IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: 100, maxQueuingActions: 50);

        #endregion
    }

    public static void Bulkhead_V8()
    {
        #region migration-bulkhead-v8

        // Create pipeline with concurrency limiter. Because ResiliencePipeline supports both sync and async
        // callbacks, there is no need to define it twice.
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddConcurrencyLimiter(permitLimit: 100, queueLimit: 50)
            .Build();

        // Create a generic pipeline with concurrency limiter. Because ResiliencePipeline<T> supports both sync and async
        // callbacks, there is no need to define it twice.
        ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddConcurrencyLimiter(permitLimit: 100, queueLimit: 50)
            .Build();

        #endregion
    }
}
