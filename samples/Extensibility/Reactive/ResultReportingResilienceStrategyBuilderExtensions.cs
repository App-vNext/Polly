using Polly;

namespace Extensibility.Reactive;

#pragma warning disable IDE0022 // Use expression body for method

#region ext-reactive-extensions

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

#endregion
