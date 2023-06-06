namespace Polly;

/// <summary>
/// Extensions for <see cref="ResilienceStrategyBuilderBase"/>.
/// </summary>
public static class ResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Adds an already created strategy instance to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this builder was already used to create a strategy. The builder cannot be modified after it has been used.</exception>
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, ResilienceStrategy strategy)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(strategy);

        builder.AddStrategy(_ => strategy, EmptyOptions.Instance);
        return builder;
    }

    internal sealed class EmptyOptions : ResilienceStrategyOptions
    {
        public static readonly EmptyOptions Instance = new();

        public override string StrategyType => "Empty";
    }
}
