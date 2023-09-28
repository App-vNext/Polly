using Polly.Utils;

namespace Polly;

/// <summary>
/// Testing related extensions for resilience pipeline builder.
/// </summary>
public static class TestingResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// Updates a <see cref="TimeProvider"/> that the builder uses.
    /// </summary>
    /// <typeparam name="TBuilder">The resilience pipeline builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="timeProvider">The time provider instance.</param>
    /// <returns>The same builder instance.</returns>
    public static TBuilder WithTimeProvider<TBuilder>(this TBuilder builder, TimeProvider timeProvider)
        where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(timeProvider);

        builder.TimeProvider = timeProvider;
        return builder;
    }
}
