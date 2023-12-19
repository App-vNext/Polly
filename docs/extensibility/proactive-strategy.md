# Proactive resilience strategy

This document guides you in creating a **Timing resilience strategy** that tracks the execution times of callbacks and reports when the execution time exceeds the expected duration. This is a prime example of a proactive strategy because we aren't concerned with the individual results produced by the callbacks. Hence, this strategy can be used across various result types.

## Implementation

Proactive resilience strategies are derived from the [`ResilienceStrategy`](xref:Polly.ResilienceStrategy) base class. For this strategy, the implementation is:

<!-- snippet: ext-proactive-strategy -->
```cs
// Strategies should be internal and not exposed in the library's public API.
// Configure the strategy through extension methods and options.
internal sealed class TimingResilienceStrategy : ResilienceStrategy
{
    private readonly TimeSpan _threshold;
    private readonly Func<OnThresholdExceededArguments, ValueTask>? _onThresholdExceeded;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public TimingResilienceStrategy(
        TimeSpan threshold,
        Func<OnThresholdExceededArguments, ValueTask>? onThresholdExceeded,
        ResilienceStrategyTelemetry telemetry)
    {
        _threshold = threshold;
        _telemetry = telemetry;
        _onThresholdExceeded = onThresholdExceeded;
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var stopwatch = Stopwatch.StartNew();

        // Execute the given callback and adhere to the ContinueOnCapturedContext property value.
        Outcome<TResult> outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        if (stopwatch.Elapsed > _threshold)
        {
            // Bundle information about the event into arguments.
            var args = new OnThresholdExceededArguments(context, _threshold, stopwatch.Elapsed);

            // Report this as a resilience event if the execution took longer than the threshold.
            _telemetry.Report(
                new ResilienceEvent(ResilienceEventSeverity.Warning, "ExecutionThresholdExceeded"),
                context,
                args);

            if (_onThresholdExceeded is not null)
            {
                await _onThresholdExceeded(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }
        }

        // Return the outcome directly.
        return outcome;
    }
}
```
<!-- endSnippet -->

Review the code and comments to understand the implementation. Take note of the `OnThresholdExceededArguments` struct:

<!-- snippet: ext-proactive-args -->
```cs
// Structs for arguments encapsulate details about specific events within the resilience strategy.
// Relevant properties to the event can be exposed. In this event, the actual execution time and the exceeded threshold are included.
public readonly struct OnThresholdExceededArguments
{
    public OnThresholdExceededArguments(ResilienceContext context, TimeSpan threshold, TimeSpan duration)
    {
        Context = context;
        Threshold = threshold;
        Duration = duration;
    }

    public TimeSpan Threshold { get; }

    public TimeSpan Duration { get; }

    // As per convention, all arguments should provide a "Context" property.
    public ResilienceContext Context { get; }
}
```
<!-- endSnippet -->

Arguments should always have an `Arguments` suffix and include a `Context` property. Using arguments boosts the extensibility and maintainability of the API, as adding new members becomes a non-breaking change. The `OnThresholdExceededArguments` provides details about the actual execution time and threshold, allowing consumers to respond to this event or supply a custom callback for such situations.

## Options

In the previous section, we implemented the `TimingResilienceStrategy`. Now, it's time to integrate it with Polly and its public API.

Let's define the public `TimingStrategyOptions` to configure our strategy:

<!-- snippet: ext-proactive-options -->
```cs
public class TimingStrategyOptions : ResilienceStrategyOptions
{
    public TimingStrategyOptions()
    {
        // Assign a default name to the options for more detailed telemetry insights.
        Name = "Timing";
    }

    // Apply validation attributes to guarantee the options' validity.
    // The pipeline will handle validation automatically during its construction.
    [Range(typeof(TimeSpan), "00:00:00", "1.00:00:00")]
    [Required]
    public TimeSpan? Threshold { get; set; }

    // Provide the delegate to be called when the threshold is surpassed.
    // Ideally, arguments should share the delegate's name, but with an "Arguments" suffix.
    public Func<OnThresholdExceededArguments, ValueTask>? OnThresholdExceeded { get; set; }
}
```
<!-- endSnippet -->

Options represent our public contract with the consumer. By using them, we can easily add new members without breaking changes and perform validation consistently.

## Extensions

So far, we've covered:

- The public `TimingStrategyOptions` and its associated arguments.
- The proactive strategy implementation named `TimingResilienceStrategy`.

The final step is to integrate these components with each other by adding new extensions for both `ResiliencePipelineBuilder` and `ResiliencePipelineBuilder<T>`. Since both builders inherit from the same base class, we can introduce a single extension for `ResiliencePipelineBuilderBase` to serve both.

<!-- snippet: ext-proactive-extensions -->
```cs
public static class TimingResilienceStrategyBuilderExtensions
{
    // The extensions should return the builder to support a fluent API.
    // For proactive strategies, we can target both "ResiliencePipelineBuilderBase" and "ResiliencePipelineBuilder<T>"
    // using generic constraints.
    public static TBuilder AddTiming<TBuilder>(this TBuilder builder, TimingStrategyOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        // Add the strategy through the AddStrategy method. This method accepts a factory delegate
        // and automatically validates the options.
        return builder.AddStrategy(
            context =>
            {
                // The "context" provides various properties for the strategy's use.
                // In this case, we simply use the "Telemetry" property and pass it to the strategy.
                // The Threshold and OnThresholdExceeded values are sourced from the options.
                var strategy = new TimingResilienceStrategy(
                    options.Threshold!.Value,
                    options.OnThresholdExceeded,
                    context.Telemetry);

                return strategy;
            },
            options);
    }
}
```
<!-- endSnippet -->

## Usage

<!-- snippet: ext-proactive-strategy-usage -->
```cs
// Add the proactive strategy to the builder
var pipeline = new ResiliencePipelineBuilder()
    // This is custom extension defined in this sample
    .AddTiming(new TimingStrategyOptions
    {
        Threshold = TimeSpan.FromSeconds(1),
        OnThresholdExceeded = args =>
        {
            Console.WriteLine("Execution threshold exceeded!");
            return default;
        },
    })
    .Build();
```
<!-- endSnippet -->

## Resources

For further information on proactive resilience strategies, consider exploring these resources:

- [Timing strategy sample](https://github.com/App-vNext/Polly/tree/main/samples/Extensibility/Proactive): A practical example from this guide.
- [Timeout resilience strategy](https://github.com/App-vNext/Polly/tree/main/src/Polly.Core/Timeout): Discover the built-in timeout resilience strategy implementation.
- [Rate limiter resilience strategy](https://github.com/App-vNext/Polly/tree/main/src/Polly.RateLimiting): Discover how rate limiter strategy is implemented.
