using Polly;

namespace Extensibility.Proactive;

#pragma warning disable IDE0022 // Use expression body for method

#region ext-proactive-extensions

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

#endregion
