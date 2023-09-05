using Polly;

namespace Snippets.Docs;

internal static class ResilienceContextUsage
{
    public static async Task ResilienceContextSample()
    {
        var cancellationToken = CancellationToken.None;

        #region resilience-context

        // Retrieve a context with a cancellation token
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);

        // Attach custom data to the context
        var key1 = new ResiliencePropertyKey<string>("my-key-1");
        var key2 = new ResiliencePropertyKey<int>("my-key-2");

        context.Properties.Set(key1, "my-data");
        context.Properties.Set(key2, 123);

        // Utilize the context in a resilience pipeline
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                OnRetry = args =>
                {
                    // Retrieve custom data from the context, if available
                    if (args.Context.Properties.TryGetValue(key1, out var data))
                    {
                        Console.WriteLine("OnRetry, Custom Data: {0}", data);
                    }

                    return default;
                }
            })
            .Build();

        // Execute the resilience pipeline asynchronously
        await pipeline.ExecuteAsync(
            async context =>
            {
                // Insert your execution logic here
            },
            context);

        // Return the context to the pool
        ResilienceContextPool.Shared.Return(context);

        #endregion
    }

    public static async Task ResilienceContextPoolSample()
    {
        var cancellationToken = CancellationToken.None;

        #region resilience-context-pool

        // Retrieve a context with a cancellation token
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);

        // Retrieve a context with a specific operation key
        context = ResilienceContextPool.Shared.Get("my-operation-key", cancellationToken);

        // Retrieve a context with multiple properties
        context = ResilienceContextPool.Shared.Get(
            operationKey: "my-operation-key",
            continueOnCapturedContext: true,
            cancellationToken: cancellationToken);

        // Use the pool here

        // Return the context to the pool
        ResilienceContextPool.Shared.Return(context);

        #endregion
    }
}
