using Polly;

namespace Extensibility.Proactive;

#pragma warning disable IDE0022 // Use expression body for method

#region ext-proactive-extensions

public static class TimingResilienceStrategyBuilderExtensions
{
    // The extensions should return the builder for fluent API.
    // For proactive strategies we can target both "ResiliencePipelineBuilderBase" and "ResiliencePipelineBuilder<T>"
    // by using generic constraints.
    public static TBuilder AddTiming<TBuilder>(this TBuilder builder, TimingStrategyOptions options)
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

#endregion
