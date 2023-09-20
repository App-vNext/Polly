# Proactive resilience strategy

This section guides you in creating a **Timing resilience strategy** that tracks the execution times of callbacks and reports when the execution time exceeds the expected duration. This is a prime example of a proactive strategy because we aren't concerned with the individual results produced by the callbacks. Hence, this strategy can be used across various result types.

## Implementation

Proactive resilience strategies are derived from the [`ResilienceStrategy`](xref:Polly.ResilienceStrategy) base class. For this strategy, the implementation is:

<!-- snippet: ext-proactive-strategy -->
```cs
// The strategies should be internal and not exposed as part of the library's public API.
// The configuration of strategy should be done via extension methods and options.
internal sealed class TimingResilienceStrategy : ResilienceStrategy
{
    private readonly TimeSpan _threshold;

    private readonly Func<ThresholdExceededArguments, ValueTask>? _thresholdExceeded;

    private readonly ResilienceStrategyTelemetry _telemetry;

    public TimingResilienceStrategy(
        TimeSpan threshold,
        Func<ThresholdExceededArguments, ValueTask>? thresholdExceeded,
        ResilienceStrategyTelemetry telemetry)
    {
        _threshold = threshold;
        _telemetry = telemetry;
        _thresholdExceeded = thresholdExceeded;
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var stopwatch = Stopwatch.StartNew();

        // Execute the provided callback and respect the value of ContinueOnCapturedContext property.
        Outcome<TResult> outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        if (stopwatch.Elapsed > _threshold)
        {
            // Create arguments that encapsulate the information about the event.
            var args = new ThresholdExceededArguments(context, _threshold, stopwatch.Elapsed);

            // Since we detected that this execution took longer than the threshold, we will report this as an resilience event.
            _telemetry.Report(
                new ResilienceEvent(ResilienceEventSeverity.Warning, "ExecutionThresholdExceeded"), // Pass the event severity and the event name
                context, // Forward the context
                 args); // Forward the arguments so any listeners can recognize this particular event

            if (_thresholdExceeded is not null)
            {
                await _thresholdExceeded(args).ConfigureAwait(context.ContinueOnCapturedContext);
            }
        }

        // Just return the outcome
        return outcome;
    }
}
```
<!-- endSnippet -->

Review the code and comments to understand the implementation. Take note of the `ThresholdExceededArguments` struct:

<!-- snippet: ext-proactive-args -->
```cs
// Arguments-based structs encapsulate information about particular event that occurred inside resilience strategy.
// They cna expose any properties that are relevant to the event.
// For this event the actual duration of execution and the threshold that was exceeded are relevant.
public readonly struct ThresholdExceededArguments
{
    public ThresholdExceededArguments(ResilienceContext context, TimeSpan threshold, TimeSpan duration)
    {
        Context = context;
        Threshold = threshold;
        Duration = duration;
    }

    public TimeSpan Threshold { get; }

    public TimeSpan Duration { get; }

    // By convention, all arguments should expose the "Context" property.
    public ResilienceContext Context { get; }
}
```
<!-- endSnippet -->

Arguments should always have an `Arguments` suffix and include a `Context` property. Using arguments boosts the extensibility and maintainability of the API, as adding new members becomes a non-breaking change. The `ThresholdExceededArguments` provides details about the actual execution time and threshold, allowing listeners to respond to this event or supply a custom callback for such situations.

## Options

In the previous section, we implemented the `TimingResilienceStrategy`. Now, it's time to integrate it with Polly and its public API.

Let's define the public `TimeoutStrategyOptions` to configure our strategy:

<!-- snippet: ext-proactive-options -->
```cs
public class TimeoutStrategyOptions : ResilienceStrategyOptions
{
    public TimeoutStrategyOptions()
    {
        // It's recommended to set the default name for the options so
        // the consumer can get additional information in the telemetry.
        Name = "Timing";
    }

    // You can use the validation attributes to ensure the options are valid.
    // The validation will be performed automatically when building the pipeline.
    [Range(typeof(TimeSpan), "00:00:00", "1.00:00:00")]
    [Required]
    public TimeSpan? Threshold { get; set; }

    // Expose the delegate that will be invoked when the threshold is exceeded.
    // The recommendation is that the arguments should have the same name as the delegate but with "Arguments" suffix.
    // Notice that the delegate is not required.
    public Func<ThresholdExceededArguments, ValueTask>? ThresholdExceeded { get; set; }
}
```
<!-- endSnippet -->

Options represent our public contract with the consumer. By using them, we can easily add new members without breaking changes and perform validation consistently.

## Extensions

So far, we have:

- Public `TimeoutStrategyOptions` and the public arguments associated with them.
- Our proactive strategy implementation - `TimingResilienceStrategy`.

The last step is to combine these components by introducing new extensions for the `ResiliencePipelineBuilder` and `ResiliencePipelineBuilder<T>`. As both builders share the same base class, we can present a single extension for `ResiliencePipelineBuilderBase` to cater to both.

<!-- snippet: ext-proactive-extensions -->
```cs
public static class TimingResilienceStrategyBuilderExtensions
{
    // The extensions should return the builder for fluent API.
    // For proactive strategies we can target both "ResiliencePipelineBuilderBase" and "ResiliencePipelineBuilder<T>"
    // by using generic constraints.
    public static TBuilder AddTiming<TBuilder>(this TBuilder builder, TimeoutStrategyOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        // The strategy should be added via AddStrategy method that accepts a factory delegate
        // and validates the options automatically.

        return builder.AddStrategy(
            context =>
            {
                // The "context" contains various properties that can be used by the strategy.
                // Here, we just use the "Telemetry" and pass it to the strategy.
                // The Threshold and ThresholdExceeded is passed from the options.
                var strategy = new TimingResilienceStrategy(
                    options.Threshold!.Value,
                    options.ThresholdExceeded,
                    context.Telemetry);

                return strategy;
            },
            options);
    }
}
```
<!-- endSnippet -->

## Resources

For further understanding of proactive resilience strategies, consider exploring these resources:

- [Timing strategy sample](https://github.com/App-vNext/Polly/tree/main/samples/Extensibility/Proactive): A practical example from this guide.
- [Timeout resilience strategy](https://github.com/App-vNext/Polly/tree/main/src/Polly.Core/Timeout): Discover the built-in timeout resilience strategy implementation.
- [Rate limiter resilience strategy](https://github.com/App-vNext/Polly/tree/main/src/Polly.RateLimiting): DIscover how rate limiter strategy is implemented.
