using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;
using Polly.Timeout;

namespace Snippets.Docs;

#pragma warning disable CA1031 // Do not catch general exception types

internal static class ResiliencePipelines
{
    public static async Task Usage()
    {
        var cancellationToken = CancellationToken.None;
        var httpClient = new HttpClient();
        var endpoint = new Uri("https://endpoint");

        #region resilience-pipeline-usage

        // Creating a new resilience pipeline
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddConcurrencyLimiter(100)
            .Build();

        // Executing an asynchronous void callback
        await pipeline.ExecuteAsync(
            async token => await MyMethodAsync(token),
            cancellationToken);

        // Executing a synchronous void callback
        pipeline.Execute(() => MyMethod());

        // Executing an asynchronous callback that returns a value
        await pipeline.ExecuteAsync(
            async token => await httpClient.GetAsync(endpoint, token),
            cancellationToken);

        // Executing an asynchronous callback without allocating a lambda
        await pipeline.ExecuteAsync(
            static async (state, token) => await state.httpClient.GetAsync(state.endpoint, token),
            (httpClient, endpoint),  // State provided here
            cancellationToken);

        // Executing an asynchronous callback and passing custom data

        // 1. Retrieve a context from the shared pool
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);

        // 2. Add custom data to the context
        context.Properties.Set(new ResiliencePropertyKey<string>("my-custom-data"), "my-custom-data");

        // 3. Execute the callback
        await pipeline.ExecuteAsync(static async context =>
        {
            // Retrieve custom data from the context
            var customData = context.Properties.GetValue(
                new ResiliencePropertyKey<string>("my-custom-data"),
                "default-value");

            Console.WriteLine("Custom Data: {0}", customData);

            await MyMethodAsync(context.CancellationToken);
        },
        context);

        // 4. Optionally, return the context to the shared pool
        ResilienceContextPool.Shared.Return(context);

        #endregion
    }

    #region resilience-pipeline-di-usage

    public static void ConfigureMyPipelines(IServiceCollection services)
    {
        services.AddResiliencePipeline("pipeline-A", builder => builder.AddConcurrencyLimiter(100));
        services.AddResiliencePipeline("pipeline-B", builder => builder.AddRetry(new()));

        // Later, resolve the pipeline by name using ResiliencePipelineProvider<string> or ResiliencePipelineRegistry<string>
        var pipelineProvider = services.BuildServiceProvider().GetRequiredService<ResiliencePipelineProvider<string>>();
        pipelineProvider.GetPipeline("pipeline-A").Execute(() => { });
    }

    #endregion

    public static void ResiliencePipelinesDiagramRetryTimeout()
    {
        #region resilience-pipeline-diagram-retry-timeout

        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new() { ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>() }) // outer
            .AddTimeout(TimeSpan.FromSeconds(1)) // inner
            .Build();

        #endregion
    }

    public static void ResiliencePipelinesDiagramTimeoutRetry()
    {
        #region resilience-pipeline-diagram-timeout-retry

        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(10)) // outer
            .AddRetry(new()) // inner
            .Build();

        #endregion
    }

    public static void ResiliencePipelinesDiagramTimeoutRetryTimeout()
    {
        #region resilience-pipeline-diagram-timeout-retry-timeout

        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(10)) // outer most
            .AddRetry(new() { ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>() })
            .AddTimeout(TimeSpan.FromSeconds(1)) // inner most
            .Build();

        #endregion
    }

    public static async Task ExecuteOutcomeAsync()
    {
        var pipeline = ResiliencePipeline.Empty;

        #region resilience-pipeline-outcome

        // Acquire a ResilienceContext from the pool
        ResilienceContext context = ResilienceContextPool.Shared.Get();

        // Execute the pipeline and store the result in an Outcome<bool>
        Outcome<bool> outcome = await pipeline.ExecuteOutcomeAsync(
            static async (context, state) =>
            {
                Console.WriteLine("State: {0}", state);

                try
                {
                    await MyMethodAsync(context.CancellationToken);

                    // Use static utility methods from Outcome to easily create an Outcome<T> instance
                    return Outcome.FromResult(true);
                }
                catch (Exception e)
                {
                    // Create an Outcome<T> instance that holds the exception
                    return Outcome.FromException<bool>(e);
                }
            },
            context,
            "my-state");

        // Return the acquired ResilienceContext to the pool
        ResilienceContextPool.Shared.Return(context);

        // Evaluate the outcome
        if (outcome.Exception is not null)
        {
            Console.WriteLine("Execution Failed: {0}", outcome.Exception.Message);
        }
        else
        {
            Console.WriteLine("Execution Result: {0}", outcome.Result);
        }

        #endregion
    }

    private static Task MyMethodAsync(CancellationToken cancellationToken) => Task.Delay(1000, cancellationToken);

    private static void MyMethod()
    {
        // Do nothing
    }
}

