using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Polly.Timeout;

// ------------------------------------------------------------------------
// 1. Register your resilience pipeline
// ------------------------------------------------------------------------

var serviceProvider = new ServiceCollection()
    .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
    // Use "AddResiliencePipeline" extension method to configure your named pipeline
    .AddResiliencePipeline("my-pipeline", (builder, context) =>
    {
        // You can resolve any service from DI when building the pipeline
        context.ServiceProvider.GetRequiredService<ILoggerFactory>();

        builder.AddTimeout(TimeSpan.FromSeconds(1));
    })
    // You can also register result-based (generic) resilience pipelines
    // First generic parameter is the key type, the second one is the result type
    // This overload does not use the context argument (simple scenarios)
    .AddResiliencePipeline<string, HttpResponseMessage>("my-http-pipeline", builder =>
    {
        builder.AddTimeout(TimeSpan.FromSeconds(1));
    })
    .BuildServiceProvider();

// ------------------------------------------------------------------------
// 2. Retrieve and use your resilience pipeline
// ------------------------------------------------------------------------

// Resolve the resilience pipeline provider for string-based keys
ResiliencePipelineProvider<string> pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

// Retrieve the pipeline by name
ResiliencePipeline pipeline = pipelineProvider.GetPipeline("my-pipeline");

// Retrieve the generic pipeline by name
ResiliencePipeline<HttpResponseMessage> genericPipeline = pipelineProvider.GetPipeline<HttpResponseMessage>("my-http-pipeline");

try
{
    // Execute the pipeline
    // Notice in console output that telemetry is automatically enabled
    await pipeline.ExecuteAsync(async token => await Task.Delay(10000, token), CancellationToken.None);
}
catch (TimeoutRejectedException)
{
    // The timeout pipeline cancels the user callback and throws this exception
    Console.WriteLine("Timeout!");
}
