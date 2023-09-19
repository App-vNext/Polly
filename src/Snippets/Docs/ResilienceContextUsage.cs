namespace Snippets.Docs;

internal static class ResilienceContextUsage
{
    #region resilience-keys

    public static class MyResilienceKeys
    {
        public static readonly ResiliencePropertyKey<string> Key1 = new("my-key-1");

        public static readonly ResiliencePropertyKey<int> Key2 = new("my-key-2");
    }

    #endregion

    public static async Task Usage()
    {
        var cancellationToken = CancellationToken.None;

        #region resilience-context

        // Retrieve a context with a cancellation token
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);

        // Attach custom data to the context
        context.Properties.Set(MyResilienceKeys.Key1, "my-data");
        context.Properties.Set(MyResilienceKeys.Key2, 123);

        // Utilize the context in a resilience pipeline
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                OnRetry = static args =>
                {
                    // Retrieve custom data from the context, if available
                    if (args.Context.Properties.TryGetValue(MyResilienceKeys.Key1, out var data))
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

    public static async Task Pooling()
    {
        var cancellationToken = CancellationToken.None;

        #region resilience-context-pool

        // Retrieve a context with a cancellation token
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);

        try
        {
            // Retrieve a context with a specific operation key
            context = ResilienceContextPool.Shared.Get("my-operation-key", cancellationToken);

            // Retrieve a context with multiple properties
            context = ResilienceContextPool.Shared.Get(
                operationKey: "my-operation-key",
                continueOnCapturedContext: true,
                cancellationToken: cancellationToken);

            // Use the pool here
        }
        finally
        {
            // Returning the context back to the pool is recommended, but not required as it reduces the allocations.
            // It is also OK to not return the context in case of exceptions, if you want to avoid try-catch blocks.
            ResilienceContextPool.Shared.Return(context);
        }

        #endregion
    }
}
