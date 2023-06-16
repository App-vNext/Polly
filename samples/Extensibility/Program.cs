using Polly;
using Polly.Telemetry;

// ------------------------------------------------------------------------
// Usage of custom strategy
// ------------------------------------------------------------------------
var strategy = new ResilienceStrategyBuilder()
    // This is custom extension defined in this sample
    .AddMyResilienceStrategy(new MyResilienceStrategyOptions
    {
        OnCustomEvent = args =>
        {
            Console.WriteLine("OnCustomEvent");
            return default;
        }
    })
    .Build();

// Execute the strategy
strategy.Execute(() => { });

// ------------------------------------------------------------------------
// SIMPLE EXTENSIBILITY MODEL (INLINE STRATEGY)
// ------------------------------------------------------------------------

strategy = new ResilienceStrategyBuilder()
    // Just add the strategy instance directly
    .AddStrategy(new MySimpleStrategy())
    .Build();

// Execute the strategy
strategy.Execute(() => { });

internal class MySimpleStrategy : ResilienceStrategy
{
    protected override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
    {
        Console.WriteLine("MySimpleStrategy executing!");

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
public class MyResilienceStrategyOptions : ResilienceStrategyOptions
{
    public override string StrategyType => "MyCustomStrategy";

    // Use the arguments in the delegates.
    // The recommendation is to use asynchronous delegates.
    public Func<OnCustomEventArguments, ValueTask>? OnCustomEvent { get; set; }
}

// ------------------------------------------------------------------------
// 2. Create a custom resilience strategy that derives from ResilienceStrategy
// ------------------------------------------------------------------------

// The strategy should be internal
internal class MyResilienceStrategy : ResilienceStrategy
{
    private readonly ResilienceStrategyTelemetry telemetry;
    private readonly Func<OnCustomEventArguments, ValueTask>? onCustomEvent;

    public MyResilienceStrategy(ResilienceStrategyTelemetry telemetry, MyResilienceStrategyOptions options)
    {
        this.telemetry = telemetry;
        this.onCustomEvent = options.OnCustomEvent;
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
    {
        // Here, do something before callback execution
        // ...

        // Execute the provided callback
        var outcome = await callback(context, state);

        // Here, do something after callback execution
        // ...

        // You should report important telemetry events
        telemetry.Report("MyCustomEvent", context, new OnCustomEventArguments(context));

        // Call the delegate if provided by the user
        if (onCustomEvent is not null)
        {
            await onCustomEvent(new OnCustomEventArguments(context));
        }

        return outcome;
    }
}

// ------------------------------------------------------------------------
// 3. Expose new extensions for ResilienceStrategyBuilder
// ------------------------------------------------------------------------

public static class MyResilienceStrategyExtensions
{
    // Add new extension that works for both "ResilienceStrategyBuilder" and "ResilienceStrategyBuilder<T>"
    public static TBuilder AddMyResilienceStrategy<TBuilder>(this TBuilder builder, MyResilienceStrategyOptions options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        // Validate arguments
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        builder.AddStrategy(
            // Provide a factory that creates the strategy
            context => new MyResilienceStrategy(context.Telemetry, options),

            // Pass the options, note that the options instance is automatically validated by the builder
            options);

        return builder;
    }
}


