# Polly.Extensions Overview

`Polly.Extensions` provides a set of features that streamline the integration of Polly with the standard `IServiceCollection` Dependency Injection (DI) container. It further enhances telemetry by exposing a `ConfigureTelemetry` extension method that enables [logging](https://learn.microsoft.com/dotnet/core/extensions/logging?tabs=command-line) and [metering](https://learn.microsoft.com/dotnet/core/diagnostics/metrics) for all strategies created via DI extension points.

Below is an example illustrating the usage of `AddResiliencePipeline` extension method:

<!-- snippet: add-resilience-pipeline -->
```cs
var services = new ServiceCollection();

// Define a resilience pipeline
services.AddResiliencePipeline(
  "my-key",
  builder => builder.AddTimeout(TimeSpan.FromSeconds(10)));

// Define a resilience pipeline with custom options
services
    .Configure<MyTimeoutOptions>(options => options.Timeout = TimeSpan.FromSeconds(10))
    .AddResiliencePipeline(
        "my-timeout",
        (builder, context) =>
        {
            var myOptions = context.GetOptions<MyTimeoutOptions>();

            builder.AddTimeout(myOptions.Timeout);
        });

// Resolve the resilience pipeline
var serviceProvider = services.BuildServiceProvider();
var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
var pipeline = pipelineProvider.GetPipeline("my-key");

// Use it
await pipeline.ExecuteAsync(async cancellation => await Task.Delay(100, cancellation));
```
<!-- endSnippet -->

> [!NOTE]
> Telemetry is enabled by default when utilizing the `AddResiliencePipeline(...)` extension method.

## Telemetry Features

This project implements the `TelemetryListener` and uses it to bridge the Polly-native events into logs and metrics.

Explore [telemetry documentation](../../docs/telemetry.md) for more details.
