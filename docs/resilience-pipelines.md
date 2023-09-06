# Resilience pipelines

The `ResiliencePipeline` allows executing arbitrary user-provided callbacks. It is a combination of one or more resilience strategies.

## Usage

The `ResiliencePipeline` allow executing various synchronous and asynchronous user-provided callbacks as seen in the examples below:

<!-- snippet: resilience-pipeline-usage -->
```cs
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
```
<!-- endSnippet -->

The above samples demonstrate how to use the resilience pipeline within the same scope. Additionally, consider the following:

- Separate the resilience pipeline's definition from its usage. Inject pipelines into the code that will consume them. This [facilitates various unit-testing scenarios](https://github.com/App-vNext/Polly/wiki/Unit-testing-with-Polly---with-examples).
- If your application uses Polly in multiple locations, define all pipelines at startup using [`ResiliencePipelineRegistry`](/docs/resilience-pipeline-registry.md) or using the `AddResiliencePipeline` extension. This is a common approach in .NET Core applications. For example, you could create your own extension method on `IServiceCollection` to configure pipelines consumed elsewhere in your application.

<!-- snippet: resilience-pipeline-di-usage -->
```cs
public static void ConfigureMyPipelines(IServiceCollection services)
{
    services.AddResiliencePipeline("pipeline-A", builder => builder.AddConcurrencyLimiter(100));
    services.AddResiliencePipeline("pipeline-B", builder => builder.AddRetry(new()));

    // Later, resolve the pipeline by name using ResiliencePipelineProvider<string> or ResiliencePipelineRegistry<string>
    var pipelineProvider = services.BuildServiceProvider().GetRequiredService<ResiliencePipelineProvider<string>>();
    pipelineProvider.GetPipeline("pipeline-A").Execute(() => { });
}
```
<!-- endSnippet -->

## Empty resilience pipeline

The empty resilience pipeline is a special construct that lacks any resilience strategies. You can access it through the following ways:

- `ResiliencePipeline.Empty`
- `ResiliencePipeline<T>.Empty`

This is particularly useful in test scenarios where implementing resilience strategies could slow down the test execution or over-complicate test setup.

## Retrieving execution results with `Outcome<T>`

The `ResiliencePipeline` class provides the `ExecuteOutcomeAsync(...)` method, which is designed to never throw exceptions. Instead, it stores either the result or the exception within an `Outcome<T>` struct.

<!-- snippet: resilience-pipeline-outcome -->
```cs
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
```
<!-- endSnippet -->

Use `ExecuteOutcomeAsync(...)` in high-performance scenarios where you wish to avoid re-throwing exceptions. Keep in mind that Polly's resilience strategies also make use of the `Outcome` struct to prevent unnecessary exception throwing.
