# Reactive resilience strategy

This document describes how to set up a **Result reporting resilience strategy**. This strategy lets you listen for specific results and report them to other components. It serves as a good example of a reactive strategy because it deals with specific results.

## Implementation

Reactive resilience strategies inherit from the [`ResilienceStrategy<T>`](xref:Polly.ResilienceStrategy`1) base class. The implementation for this specific strategy:

<!-- snippet: ext-reactive-strategy -->
```cs
// Strategies should be internal and not exposed in the library's public API.
// Use extension methods and options to configure the strategy.
internal sealed class ResultReportingResilienceStrategy<T> : ResilienceStrategy<T>
{
    private readonly Func<ResultReportingPredicateArguments<T>, ValueTask<bool>> _shouldHandle;
    private readonly Func<OnReportResultArguments<T>, ValueTask> _onReportResult;
    private readonly ResilienceStrategyTelemetry _telemetry;

    public ResultReportingResilienceStrategy(
        Func<ResultReportingPredicateArguments<T>, ValueTask<bool>> shouldHandle,
        Func<OnReportResultArguments<T>, ValueTask> onReportResult,
        ResilienceStrategyTelemetry telemetry)
    {
        _shouldHandle = shouldHandle;
        _onReportResult = onReportResult;
        _telemetry = telemetry;
    }

    protected override async ValueTask<Outcome<T>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state)
    {
        // Execute the given callback and adhere to the ContinueOnCapturedContext property value.
        Outcome<T> outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        // Check if the outcome should be reported using the "ShouldHandle" predicate.
        if (await _shouldHandle(new ResultReportingPredicateArguments<T>(context, outcome)).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            // Bundle information about the event into arguments.
            var args = new OnReportResultArguments<T>(context, outcome);

            // Report this as a resilience event with information severity level to the telemetry infrastructure.
            _telemetry.Report(
                new ResilienceEvent(ResilienceEventSeverity.Information, "ResultReported"),
                context,
                outcome,
                args);

            // Call the "OnReportResult" callback.
            await _onReportResult(args).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        return outcome;
    }
}
```
<!-- endSnippet -->

Reactive strategies use the `ShouldHandle` predicate to decide whether to handle the outcome of a user callback. The convention is to name the predicate's arguments using the `{StrategyName}PredicateArguments` pattern and return a `ValueTask<bool>`. Here, we use `ResultReportingPredicateArguments<TResult>`:

<!-- snippet: ext-reactive-predicate-args -->
```cs
public readonly struct ResultReportingPredicateArguments<TResult>
{
    public ResultReportingPredicateArguments(ResilienceContext context, Outcome<TResult> outcome)
    {
        Context = context;
        Outcome = outcome;
    }

    // Always include the "Context" property in the arguments.
    public ResilienceContext Context { get; }

    // Always have the "Outcome" property in reactive arguments.
    public Outcome<TResult> Outcome { get; }
}
```
<!-- endSnippet -->

Reactive arguments should **always** contain the `Context` and `Outcome` properties.

Additionally, to report the outcome, the strategy uses `OnReportResultArguments<TResult>`:

<!-- snippet: ext-reactive-event-args -->
```cs
public readonly struct OnReportResultArguments<TResult>
{
    public OnReportResultArguments(ResilienceContext context, Outcome<TResult> outcome)
    {
        Context = context;
        Outcome = outcome;
    }

    // Always include the "Context" property in the arguments.
    public ResilienceContext Context { get; }

    // Always have the "Outcome" property in reactive arguments.
    public Outcome<TResult> Outcome { get; }
}
```
<!-- endSnippet -->

Using arguments in callbacks supports a more maintainable and extensible API.

## Options

In the previous section, we implemented the `ResultReportingResilienceStrategy<T>`. Now, we need to integrate it with Polly and its public API.

Define the public `ResultReportingStrategyOptions<TResult>` class to configure our strategy:

<!-- snippet: ext-reactive-options -->
```cs
public class ResultReportingStrategyOptions<TResult> : ResilienceStrategyOptions
{
    public ResultReportingStrategyOptions()
    {
        // Assign a default name to the options for more detailed telemetry insights.
        Name = "ResultReporting";
    }

    // Options for reactive strategies should always include a "ShouldHandle" delegate.
    // Set a sensible default when possible. Here, we handle all exceptions.
    public Func<ResultReportingPredicateArguments<TResult>, ValueTask<bool>> ShouldHandle { get; set; } = args =>
    {
        return new ValueTask<bool>(args.Outcome.Exception is not null);
    };

    // This illustrates an event delegate. Note that the arguments struct carries the same name as the delegate but with an "Arguments" suffix.
    // The event follows the async convention and must be set by the user.
    //
    // The [Required] attribute enforces the consumer to specify this property, used when some properties do not have sensible defaults and are required.
    [Required]
    public Func<OnReportResultArguments<TResult>, ValueTask>? OnReportResult { get; set; }
}
```
<!-- endSnippet -->

If you want to support non-generic options for the `ResiliencePipelineBuilder`, you can expose them as well:

<!-- snippet: ext-reactive-non-generic-options -->
```cs
// Simply derive from the generic options, using 'object' as the result type.
// This allows the strategy to manage all results.
public class ResultReportingStrategyOptions : ResultReportingStrategyOptions<object>
{
}
```
<!-- endSnippet -->

Using options as a public contract helps us ensure flexibility with consumers. By adopting this method, you can introduce new members with ease without introducing breaking changes and maintain consistent validation.

## Extensions

Up until now, we've discussed:

- The public `ResultReportingStrategyOptions<TResult>` and the related arguments.
- The proactive strategy implementation called `ResultReportingResilienceStrategy<TResult>`.

The next step is to combine these elements by introducing new extensions for `ResiliencePipelineBuilder<T>` and, optionally, `ResiliencePipelineBuilder`.

<!-- snippet: ext-reactive-extensions -->
```cs
public static class ResultReportingResilienceStrategyBuilderExtensions
{
    // Add extensions for the generic builder.
    // Extensions should return the builder to support a fluent API.
    public static ResiliencePipelineBuilder<TResult> AddResultReporting<TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        ResultReportingStrategyOptions<TResult> options)
    {
        // Add the strategy through the AddStrategy method. This method accepts a factory delegate
        // and automatically validates the options.
        return builder.AddStrategy(
            context =>
            {
                // The "context" provides various properties for the strategy's use.
                // In this case, we simply use the "Telemetry" property and pass it to the strategy.
                // The ShouldHandle and OnReportResult values are sourced from the options.
                var strategy = new ResultReportingResilienceStrategy<TResult>(
                    options.ShouldHandle,
                    options.OnReportResult!,
                    context.Telemetry);

                return strategy;
            },
            options);
    }

    // Optionally, if suitable for the strategy, add support for non-generic builders.
    // Observe the use of the non-generic ResultReportingStrategyOptions.
    public static ResiliencePipelineBuilder AddResultReporting(
        this ResiliencePipelineBuilder builder,
        ResultReportingStrategyOptions options)
    {
        return builder.AddStrategy(
            context =>
            {
                var strategy = new ResultReportingResilienceStrategy<object>(
                    options.ShouldHandle,
                    options.OnReportResult!,
                    context.Telemetry);

                return strategy;
            },
            options);
    }
}
```
<!-- endSnippet -->

## Usage

<!-- snippet: ext-reactive-strategy-usage -->
```cs
// Add reactive strategy to the builder
new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddResultReporting(new ResultReportingStrategyOptions<HttpResponseMessage>
    {
        // Define what outcomes to handle
        ShouldHandle = args => args.Outcome switch
        {
            { Exception: { } } => PredicateResult.True(),
            { Result.StatusCode: HttpStatusCode.InternalServerError } => PredicateResult.True(),
            _ => PredicateResult.False()
        },
        OnReportResult = args =>
        {
            Console.WriteLine($"Result: {args.Outcome}");
            return default;
        }
    });

// You can also use the non-generic ResiliencePipelineBuilder to handle any kind of result.
new ResiliencePipelineBuilder()
    .AddResultReporting(new ResultReportingStrategyOptions
    {
        // Define what outcomes to handle
        ShouldHandle = args => args.Outcome switch
        {
            { Exception: { } } => PredicateResult.True(),
            { Result: HttpResponseMessage message } when message.StatusCode == HttpStatusCode.InternalServerError => PredicateResult.True(),
            _ => PredicateResult.False()
        },
        OnReportResult = args =>
        {
            Console.WriteLine($"Result: {args.Outcome}");
            return default;
        }
    });
```
<!-- endSnippet -->

## Resources

For further information about reactive resilience strategies, consider exploring these resources:

- [Result reporting strategy sample](https://github.com/App-vNext/Polly/tree/main/samples/Extensibility/Reactive): A practical example from this guide.
- [Retry resilience strategy](https://github.com/App-vNext/Polly/tree/main/src/Polly.Core/Retry): Discover the built-in retry resilience strategy implementation.
- [Fallback resilience strategy](https://github.com/App-vNext/Polly/tree/main/src/Polly.Core/Fallback): Discover the built-in fallback resilience strategy implementation.
