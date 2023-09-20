# Proactive resilience strategy

This section will guide you through the creation of **Timing Resilience Strategy** that track execution times of callbacks and reports when the execution time took longer that expected. This is a good example of proactive strategy as we do not really care about individual results produced by callbacks. This way,  this strategy can used across all types of results.

## Implementation

Proactive resilience strategies derive from [`ResilienceStrategy`](xref:Polly.ResilienceStrategy) base class. In case of this particular strategy, the implementation is:

<!-- snippet: ext-proactive-strategy -->
```cs
// The strategies should be internal and exposed as part of the library's public API.
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

Review the code and comments to learn about the implementation. Notice the `ThresholdExceededArguments` struct:

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

The arguments should always end with `Arguments` suffix and should contain the `Context` property. Using arguments improves the extensibility and maintainability of the API as adding new members to it is a non-breaking change.

In this case the `ThresholdExceededArguments` holds information about actual execution time and threshold so any listener can react to this event or provide a custom callback to be executed when this situation occurs.

## Options

In previous section we implemented the `TimingResilienceStrategy`. Now, we need to integrate it into Polly and it's public API.

First let's define public `TimeoutStrategyOptions` that will be used to configure our strategy:

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

The options are our public contract with the consumer. Using them allows easily add new members to it without breaking changes and also perform validation in a standard way.

## Extensions

At this point we have:

- Public `TimeoutStrategyOptions` alongside public arguments used by them.
- Implementation of our proactive strategy - `TimingResilienceStrategy`.

The final part is to integrate these components by exposing new extensions for the `ResiliencePipelineBuilder` and `ResiliencePipelineBuilder<T>`. Because both builders share the same base class, we can expose a single extension for `ResiliencePipelineBuilderBase` that will work for both derived builders.

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

To learn more about proactive resilience strategies you can explore the following resources:

- [Timing strategy sample](https://github.com/App-vNext/Polly/tree/main/samples/Extensibility/Proactive): The working sample from this article.
- [Timeout resilience strategy]()
- [Rate limiter resilience strategy]()
