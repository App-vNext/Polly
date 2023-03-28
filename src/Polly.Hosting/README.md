# About Polly.Hosting

The `Polly.Hosting` enables the following features:


- Integrates the Polly with the standard `IServiceCollection` DI container.
- Implements the `ResilienceTelemetryFactory` that adds [logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line) and [metering](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics) for all strategies created using the DI extension points.


Example:

``` csharp
var services = new ServiceCollection();

// define your strategy
services.AddResilienceStrategy(
  "my-key", 
   context => context.Builder.AddTimeout(TimeSpan.FromSeconds(10)));

// define your strategy using custom options
services.AddResilienceStrategy(
    "my-timeout",
    context =>
    {
        var myOptions = context.ServiceProvider.GetRequiredService<IOptions<MyTimeoutOptions>>().Value;
        context.Builder.AddTimeout(myOptions.Timeout);
    });

// use your strategy
var serviceProvider = services.BuildServiceProvider();
var strategyProvider = serviceProvider.GetRequiredService<ResilienceStrategyProvider<string>>();

var resilienceStrategy = strategyProvider.Get("my-key");

// use your strategy
The consumer just calls the AddResilienceStrategy and provides a callback that configures the resilience strategy.
```

