using System.Net.Http;

namespace Snippets.Docs;

internal static partial class Migration
{
    public static void Timeout_V7()
    {
        #region migration-timeout-v7

        // Create sync timeout
        ISyncPolicy syncPolicy = Policy.Timeout(TimeSpan.FromSeconds(10));

        // Create async timeout
        IAsyncPolicy asyncPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10));

        // Create generic sync timeout
        ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy.Timeout<HttpResponseMessage>(TimeSpan.FromSeconds(10));

        // Create generic async timeout
        IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

        #endregion
    }

    public static void Timeout_V8()
    {
        #region migration-timeout-v8

        // Create pipeline with timeout. Because ResiliencePipeline supports both sync and async
        // callbacks, there is no need to define it twice.
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(10))
            .Build();

        // Create a generic pipeline with timeout. Because ResiliencePipeline<T> supports both sync and async
        // callbacks, there is no need to define it twice.
        ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddTimeout(TimeSpan.FromSeconds(10))
            .Build();

        #endregion
    }
}
