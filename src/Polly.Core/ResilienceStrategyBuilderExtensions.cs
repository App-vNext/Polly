using System.ComponentModel.DataAnnotations;

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

        return builder.AddStrategy(_ => strategy, EmptyOptions.Instance);
    }

    /// <summary>
    /// Adds a strategy to the builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="factory">The factory that creates a resilience strategy.</param>
    /// <param name="options">The options associated with the strategy. If none are provided the default instance of <see cref="ResilienceStrategyOptions"/> is created.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/>, <paramref name="factory"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this builder was already used to create a strategy. The builder cannot be modified after it has been used.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> is invalid.</exception>
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, Func<ResilienceStrategyBuilderContext, ResilienceStrategy> factory, ResilienceStrategyOptions options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(factory);
        Guard.NotNull(options);

        builder.AddStrategyCore(factory, options);
        return builder;
    }

    internal sealed class EmptyOptions : ResilienceStrategyOptions
    {
        public static readonly EmptyOptions Instance = new();

        public override string StrategyType => "Empty";
    }
}
