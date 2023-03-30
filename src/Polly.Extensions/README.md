# About Polly.Hosting

The `Polly.Hosting` enables the following features:


- Integrates Polly with the standard `IServiceCollection` Dependency Injection (DI) container.
- Implements `ResilienceTelemetryFactory` that adds [logging](https://learn.microsoft.com/dotnet/core/extensions/logging?tabs=command-line) and [metering](https://learn.microsoft.com/dotnet/core/diagnostics/metrics) for all strategies created using the DI extension points.

Example:

``` csharp
var services = new ServiceCollection();

// Define your strategy
services.AddResilienceStrategy(
  "my-key", 
   context => context.Builder.AddTimeout(TimeSpan.FromSeconds(10)));

// Define your strategy using custom options
services.AddResilienceStrategy(
    "my-timeout",
    context =>
    {
        var myOptions = context.ServiceProvider.GetRequiredService<IOptions<MyTimeoutOptions>>().Value;
        context.Builder.AddTimeout(myOptions.Timeout);
    });

// Use your strategy
var serviceProvider = services.BuildServiceProvider();
var strategyProvider = serviceProvider.GetRequiredService<ResilienceStrategyProvider<string>>();
var resilienceStrategy = strategyProvider.Get("my-key");
```

