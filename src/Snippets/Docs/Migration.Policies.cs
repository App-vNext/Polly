using System.Net.Http;
using Polly.Retry;

namespace Snippets.Docs;

internal static partial class Migration
{
    public static async Task Policies()
    {
        var cancellationToken = CancellationToken.None;

        #region migration-policies-v7

        // Create and use the ISyncPolicy.
        ISyncPolicy syncPolicy = Policy.Handle<Exception>().WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));
        syncPolicy.Execute(() =>
        {
            // your code here
        });

        // Create and use the IAsyncPolicy
        IAsyncPolicy asyncPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));
        await asyncPolicy.ExecuteAsync(
            async cancellationToken =>
            {
                // your code here
            },
            cancellationToken);

        // Create and use the ISyncPolicy<T>
        ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));

        syncPolicyT.Execute(() =>
        {
            // your code here
            return GetResponse();
        });

        // Create and use the IAsyncPolicy<T>
        IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));
        await asyncPolicyT.ExecuteAsync(
            async cancellationToken =>
            {
                // your code here
                return await GetResponseAsync(cancellationToken);
            },
            cancellationToken);

        #endregion
    }

    public static async Task Strategies()
    {
        var cancellationToken = CancellationToken.None;

        #region migration-policies-v8

        // Create and use the ResiliencePipeline.
        //
        // The ResiliencePipelineBuilder is used to start building the resilience pipeline,
        // instead of the static Policy.HandleException<TException>() call.
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                Delay = TimeSpan.FromSeconds(1),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Constant
            })
            .Build(); // After all necessary strategies are added, call build to create the pipeline.

        // Synchronous execution
        pipeline.Execute(() =>
        {
            // your code here
        });

        // Asynchronous execution is also supported with the same pipeline instance
        await pipeline.ExecuteAsync(async cancellationToken =>
        {
            // your code here
        },
        cancellationToken);

        // Create and use the ResiliencePipeline<T>.
        //
        // Building of generic resilience pipeline is very similar to non-generic one.
        ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
            // Notice the use of generic RetryStrategyOptions<HttpResponseMessage> to configure the strategy
            // As opposed to providing the arguments into the method.
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<Exception>()
                    .HandleResult(result => !result.IsSuccessStatusCode),
                Delay = TimeSpan.FromSeconds(1),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Constant
            })
            .Build();

        // Synchronous execution
        pipelineT.Execute(() =>
        {
            // your code here
            return GetResponse();
        });

        // Asynchronous execution
        await pipelineT.ExecuteAsync(async cancellationToken =>
        {
            // your code here
            return await GetResponseAsync(cancellationToken);
        },
        cancellationToken);

        #endregion
    }

    private static HttpResponseMessage GetResponse() => new();

    private static Task<HttpResponseMessage> GetResponseAsync(CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage());
}
