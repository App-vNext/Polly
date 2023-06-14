using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Polly.Timeout;

// ------------------------------------------------------------------------
// 1. Register your resilience strategy
// ------------------------------------------------------------------------

var serviceProvider = new ServiceCollection()
    .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
    // Use "AddResilienceStrategy" extension method to configure your named strategy
    .AddResilienceStrategy("my-strategy", (builder, context) =>
    {
        // You can resolve any service from DI when building the strategy
        context.ServiceProvider.GetRequiredService<ILoggerFactory>();

        builder.AddTimeout(TimeSpan.FromSeconds(1));
    })
    // You can also register result-based (generic) resilience strategies
    // First generic parameter is the key type, the second one is the result type
    // This overload does not use the context argument (simple scenarios)
    .AddResilienceStrategy<string, HttpResponseMessage>("my-strategy", builder =>
    {
        builder.AddTimeout(TimeSpan.FromSeconds(1));
    })
    .BuildServiceProvider();

// ------------------------------------------------------------------------
// 2. Retrieve and use your resilience strategy
// ------------------------------------------------------------------------

// Resolve the resilience strategy provider for string-based keys
ResilienceStrategyProvider<string> strategyProvider = serviceProvider.GetRequiredService<ResilienceStrategyProvider<string>>();

// Retrieve the strategy by name
ResilienceStrategy strategy = strategyProvider.Get("my-strategy");

// Retrieve the generic strategy by name
ResilienceStrategy<HttpResponseMessage> genericStrategy = strategyProvider.Get<HttpResponseMessage>("my-strategy");

try
{
    // Execute the strategy
    // Notice in console output that telemetry is automatically enabled
    await strategy.ExecuteAsync(async token => await Task.Delay(10000, token), CancellationToken.None);
}
catch (TimeoutRejectedException)
{
}
