using Polly;
using Polly.Telemetry;

// ------------------------------------------------------------------------
// Usage of custom strategy
// ------------------------------------------------------------------------
var pipeline = new ResiliencePipelineBuilder()
    // This is custom extension defined in this sample
    .AddMyResilienceStrategy(new MySimpleStrategyOptions
    {
        OnCustomEvent = args =>
        {
            Console.WriteLine("OnCustomEvent");
            return default;
        },
    })
    .Build();

// Execute the pipeline
pipeline.Execute(() => { });

// ------------------------------------------------------------------------
// SIMPLE EXTENSIBILITY MODEL (INLINE STRATEGY)
// ------------------------------------------------------------------------
pipeline = new ResiliencePipelineBuilder()
    // Just add the strategy instance directly
    .AddStrategy(_ => new MySimpleStrategy(), new MySimpleStrategyOptions())
    .Build();

// Execute the pipeline
pipeline.Execute(() => { });

internal class MySimpleStrategy : ResilienceStrategy
{
    protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        Console.WriteLine("MySimpleStrategy executing!");

        // The "state" is an ambient value passed by the caller that holds the state.
        // Here, we do not do anything with it, just pass it to the callback.

        // Execute the provided callback
        return callback(context, state);
    }
}

// ------------------------------------------------------------------------
// STANDARD EXTENSIBILITY MODEL
// ------------------------------------------------------------------------

// ------------------------------------------------------------------------
// 1. Create options for your custom strategy
// ------------------------------------------------------------------------

// 1.A Define arguments for events that your strategy uses (optional)
public readonly record struct OnCustomEventArguments(ResilienceContext Context);

// 1.B Define the options.
public class MySimpleStrategyOptions : ResilienceStrategyOptions
{
    // Use the arguments in the delegates.
    // The recommendation is to use asynchronous delegates.
    public Func<OnCustomEventArguments, ValueTask>? OnCustomEvent { get; set; }
}

// ------------------------------------------------------------------------
// 2. Create a custom resilience strategy that derives from ResilienceStrategy
// ------------------------------------------------------------------------

// The strategy should be internal and not exposed as part of any public API.
// Instead, expose options and extensions for resilience strategy builder.
//
// For reactive strategies, you can use ReactiveResilienceStrategy<T> as base class.
internal class MyResilienceStrategy : ResilienceStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly Func<OnCustomEventArguments, ValueTask>? _onCustomEvent;

    public MyResilienceStrategy(ResilienceStrategyTelemetry telemetry, MySimpleStrategyOptions options)
    {
        _telemetry = telemetry;
        _onCustomEvent = options.OnCustomEvent;
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        // Here, do something before callback execution
        // ...

        // Execute the provided callback
        var outcome = await callback(context, state);

        // Here, do something after callback execution
        // ...

        // You can then report important telemetry events
        _telemetry.Report(
            new ResilienceEvent(ResilienceEventSeverity.Information, "MyCustomEvent"),
            context,
            new OnCustomEventArguments(context));

        // Call the delegate if provided by the user
        if (_onCustomEvent is not null)
        {
            await _onCustomEvent(new OnCustomEventArguments(context));
        }

        return outcome;
    }
}

// ------------------------------------------------------------------------
// 3. Expose new extensions for ResiliencePipelineBuilder
// ------------------------------------------------------------------------
public static class MyResilienceStrategyExtensions
{
    // Add new extension that works for both "ResiliencePipelineBuilder" and "ResiliencePipelineBuilder<T>"
    public static TBuilder AddMyResilienceStrategy<TBuilder>(this TBuilder builder, MySimpleStrategyOptions options)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        return builder.AddStrategy(
            context => new MyResilienceStrategy(context.Telemetry, options),  // Provide a factory that creates the strategy
            options); // Pass the options, note that the options instance is automatically validated by the builder
    }
}
