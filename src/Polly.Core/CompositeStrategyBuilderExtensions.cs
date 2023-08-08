using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Polly;

/// <summary>
/// Extensions for <see cref="CompositeStrategyBuilderBase"/>.
/// </summary>
public static class CompositeStrategyBuilderExtensions
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
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "The EmptyOptions have nothing to validate.")]
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, ResilienceStrategy strategy)
        where TBuilder : CompositeStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(strategy);

        return builder.AddStrategy(_ => strategy, EmptyOptions.Instance);
    }

    /// <summary>
    /// Adds an already created strategy instance to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this builder was already used to create a strategy. The builder cannot be modified after it has been used.</exception>
    public static CompositeStrategyBuilder<TResult> AddStrategy<TResult>(this CompositeStrategyBuilder<TResult> builder, ResilienceStrategy<TResult> strategy)
    {
        Guard.NotNull(builder);
        Guard.NotNull(strategy);

        return builder.AddStrategy(strategy.Strategy);
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
    [RequiresUnreferencedCode(Constants.OptionsValidation)]
    public static TBuilder AddStrategy<TBuilder>(this TBuilder builder, Func<StrategyBuilderContext, ResilienceStrategy> factory, ResilienceStrategyOptions options)
        where TBuilder : CompositeStrategyBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(factory);
        Guard.NotNull(options);

        builder.AddStrategyCore(factory, options);
        return builder;
    }

    /// <summary>
    /// Adds a reactive strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="factory">The factory that creates a resilience strategy.</param>
    /// <param name="options">The options associated with the strategy. If none are provided the default instance of <see cref="ResilienceStrategyOptions"/> is created.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/>, <paramref name="factory"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this builder was already used to create a strategy. The builder cannot be modified after it has been used.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> is invalid.</exception>
    [RequiresUnreferencedCode(Constants.OptionsValidation)]
    public static CompositeStrategyBuilder AddStrategy(
        this CompositeStrategyBuilder builder, Func<StrategyBuilderContext, ReactiveResilienceStrategy<object>> factory,
        ResilienceStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(factory);
        Guard.NotNull(options);

        builder.AddStrategyCore(context => new ReactiveResilienceStrategyBridge<object>(factory(context)), options);
        return builder;
    }

    /// <summary>
    /// Adds a reactive strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="factory">The factory that creates a resilience strategy.</param>
    /// <param name="options">The options associated with the strategy. If none are provided the default instance of <see cref="ResilienceStrategyOptions"/> is created.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/>, <paramref name="factory"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this builder was already used to create a strategy. The builder cannot be modified after it has been used.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> is invalid.</exception>
    [RequiresUnreferencedCode(Constants.OptionsValidation)]
    public static CompositeStrategyBuilder<TResult> AddStrategy<TResult>(
        this CompositeStrategyBuilder<TResult> builder, Func<StrategyBuilderContext, ReactiveResilienceStrategy<TResult>> factory,
        ResilienceStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(factory);
        Guard.NotNull(options);

        builder.AddStrategyCore(context => new ReactiveResilienceStrategyBridge<TResult>(factory(context)), options);
        return builder;
    }

    internal sealed class EmptyOptions : ResilienceStrategyOptions
    {
        public static readonly EmptyOptions Instance = new();
    }
}
