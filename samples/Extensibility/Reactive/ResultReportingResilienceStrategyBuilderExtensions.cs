using Polly;

namespace Extensibility.Reactive;

#pragma warning disable IDE0022 // Use expression body for method

#region ext-reactive-extensions

public static class ResultReportingResilienceStrategyBuilderExtensions
{
    // Add extensions for the generic builder.
    // Extensions should return the builder to support a fluent API.
    public static ResiliencePipelineBuilder<TResult> AddResultReporting<TResult>(this ResiliencePipelineBuilder<TResult> builder, ResultReportingStrategyOptions<TResult> options)
    {
        // Incorporate the strategy using the AddStrategy method. This method receives a factory delegate
        // and automatically checks the options.
        return builder.AddStrategy(
            context =>
            {
                // The "context" offers various properties for the strategy to use.
                // Here, we simply use the "Telemetry" and hand it over to the strategy.
                // The ShouldHandle and OnReportResult values come from the options.
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
    public static ResiliencePipelineBuilder AddResultReporting(this ResiliencePipelineBuilder builder, ResultReportingStrategyOptions options)
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
