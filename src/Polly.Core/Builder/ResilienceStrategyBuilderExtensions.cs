namespace Polly.Builder;

/// <summary>
/// The extensions for the <see cref="IResilienceStrategyBuilder"/>.
/// </summary>
public static class ResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Adds an already created strategy instance to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="strategy">The strategy instance.</param>
    /// <param name="options">The options associated with the strategy. If none are provided the default instance of <see cref="ResilienceStrategyOptions"/> is created.</param>
    /// <returns>The same builder instance.</returns>
    public static IResilienceStrategyBuilder AddStrategy(this IResilienceStrategyBuilder builder, IResilienceStrategy strategy, ResilienceStrategyOptions? options = null)
    {
        Guard.NotNull(builder);
        Guard.NotNull(strategy);

        return builder.AddStrategy(_ => strategy, options);
    }
}
