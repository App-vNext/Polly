using System.Net.Http;

namespace Snippets.Docs;

internal static partial class Migration
{
    public static void Bulkhead_V7()
    {
        #region migration-bulkhead-v7

        Policy.Bulkhead(maxParallelization: 100, maxQueuingActions: 50);

        Policy.BulkheadAsync(maxParallelization: 100, maxQueuingActions: 50);

        Policy.Bulkhead<HttpResponseMessage>(maxParallelization: 100, maxQueuingActions: 50);

        Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: 100, maxQueuingActions: 50);

        #endregion
    }

    public static void Bulkhead_V8()
    {
        #region migration-bulkhead-v8

        new ResiliencePipelineBuilder().AddConcurrencyLimiter(permitLimit: 100, queueLimit: 50).Build();

        new ResiliencePipelineBuilder<HttpResponseMessage>().AddConcurrencyLimiter(permitLimit: 100, queueLimit: 50).Build();

        #endregion
    }
}
