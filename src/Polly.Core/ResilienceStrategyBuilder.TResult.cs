using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly;

/// <summary>
/// A builder that is used to create an instance of <see cref="ResilienceStrategy{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of result to handle.</typeparam>
/// <remarks>
/// The builder supports chaining multiple strategies into a pipeline of strategies.
/// The resulting instance of <see cref="ResilienceStrategy{TResult}"/> created by the <see cref="Build"/> call will execute the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
public class ResilienceStrategyBuilder<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceStrategyBuilder{TResult}"/> class.
    /// </summary>
    public ResilienceStrategyBuilder() => Builder = new();

    internal ResilienceStrategyBuilder(ResilienceStrategyBuilder builder) => Builder = builder;

    /// <summary>
    /// Gets or sets the name of the builder.
    /// </summary>
    /// <remarks>This property is also included in the telemetry that is produced by the individual resilience strategies.</remarks>
    [Required(AllowEmptyStrings = true)]
    public string BuilderName
    {
        get => Builder.BuilderName;
        set => Builder.BuilderName = value;
    }

    /// <summary>
    /// Gets the custom properties attached to builder options.
    /// </summary>
    public ResilienceProperties Properties => Builder.Properties;

    /// <summary>
    /// Gets or sets a <see cref="TimeProvider"/> that is used by strategies that work with time.
    /// </summary>
    /// <remarks>
    /// This property is internal until we switch to official System.TimeProvider.
    /// </remarks>
    [Required]
    internal TimeProvider TimeProvider
    {
        get => Builder.TimeProvider;
        set => Builder.TimeProvider = value;
    }

    /// <summary>
    /// Gets or sets the callback that is invoked just before the final resilience strategy is being created.
    /// </summary>
    internal Action<IList<ResilienceStrategy>>? OnCreatingStrategy
    {
        get => Builder.OnCreatingStrategy;
        set => Builder.OnCreatingStrategy = value;
    }

    internal ResilienceStrategyBuilder Builder { get; }

    /// <summary>
    /// Adds an already created strategy instance to the builder.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this builder was already used to create a strategy. The builder cannot be modified after it has been used.</exception>
    public ResilienceStrategyBuilder<TResult> AddStrategy(ResilienceStrategy strategy)
    {
        Guard.NotNull(strategy);

        Builder.AddStrategy(strategy);
        return this;
    }

    /// <summary>
    /// Adds a strategy to the builder.
    /// </summary>
    /// <param name="factory">The factory that creates a resilience strategy.</param>
    /// <param name="options">The options associated with the strategy. If none are provided the default instance of <see cref="ResilienceStrategyOptions"/> is created.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this builder was already used to create a strategy. The builder cannot be modified after it has been used.</exception>
    /// <exception cref="ValidationException">Thrown when the <paramref name="options"/> are invalid.</exception>
    public ResilienceStrategyBuilder<TResult> AddStrategy(
        Func<ResilienceStrategyBuilderContext, ResilienceStrategy> factory,
        ResilienceStrategyOptions options)
    {
        Guard.NotNull(factory);
        Guard.NotNull(options);

        Builder.AddStrategy(factory, options);

        return this;
    }

    /// <summary>
    /// Builds the resilience strategy.
    /// </summary>
    /// <returns>An instance of <see cref="ResilienceStrategy{TResult}"/>.</returns>
    /// <exception cref="ValidationException">Thrown when this builder has invalid configuration.</exception>
    public ResilienceStrategy<TResult> Build() => new(Builder.Build());
}
