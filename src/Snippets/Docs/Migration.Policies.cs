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
        ISyncPolicy syncPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));

        syncPolicy.Execute(() =>
        {
            // Your code goes here
        });

        // Create and use the IAsyncPolicy
        IAsyncPolicy asyncPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));
        await asyncPolicy.ExecuteAsync(async token =>
        {
            // Your code goes here
        }, cancellationToken);

        // Create and use the ISyncPolicy<T>
        ISyncPolicy<HttpResponseMessage> syncPolicyT = Policy<HttpResponseMessage>
            .HandleResult(result => !result.IsSuccessStatusCode)
            .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));

        syncPolicyT.Execute(() =>
        {
            // Your code goes here
            return GetResponse();
        });

        // Create and use the IAsyncPolicy<T>
        IAsyncPolicy<HttpResponseMessage> asyncPolicyT = Policy<HttpResponseMessage>
            .HandleResult(result => !result.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));

        await asyncPolicyT.ExecuteAsync(async token =>
        {
            // Your code goes here
            return await GetResponseAsync(token);
        }, cancellationToken);

        #endregion
    }

    public static async Task Strategies()
    {
        var cancellationToken = CancellationToken.None;

        #region migration-policies-v8

        // Create and use the ResiliencePipeline.
        //
        // Use the ResiliencePipelineBuilder to start building the resilience pipeline
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                Delay = TimeSpan.FromSeconds(1),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Constant
            })
            .Build(); // After all necessary strategies are added, call Build() to create the pipeline.

        // Synchronous execution
        pipeline.Execute(static () =>
        {
            // Your code goes here
        });

        // Asynchronous execution is also supported with the same pipeline instance
        await pipeline.ExecuteAsync(static async token =>
        {
            // Your code goes here
        }, cancellationToken);

        // Create and use the ResiliencePipeline<T>.
        //
        // Building of generic resilience pipeline is very similar to non-generic one.
        // Notice the use of generic RetryStrategyOptions<HttpResponseMessage> to configure the strategy.
        ResiliencePipeline<HttpResponseMessage> pipelineT = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<Exception>()
                    .HandleResult(static result => !result.IsSuccessStatusCode),
                Delay = TimeSpan.FromSeconds(1),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Constant
            })
            .Build();

        // Synchronous execution
        pipelineT.Execute(static () =>
        {
            // Your code goes here
            return GetResponse();
        });

        // Asynchronous execution
        await pipelineT.ExecuteAsync(static async token =>
        {
            // Your code goes here
            return await GetResponseAsync(token);
        }, cancellationToken);

        #endregion
    }

    private static HttpResponseMessage GetResponse() => new();

    private static Task<HttpResponseMessage> GetResponseAsync(CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage());
}
