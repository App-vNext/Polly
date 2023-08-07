using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Timeout;

namespace Polly;

/// <summary>
/// Extension methods for adding timeouts to a <see cref="CompositeStrategyBuilder"/>.
/// </summary>
public static class TimeoutCompositeStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a timeout resilience strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="timeout">The timeout value. This value should be greater than <see cref="TimeSpan.Zero"/>.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options produced from the arguments are invalid.</exception>
    public static CompositeStrategyBuilder<TResult> AddTimeout<TResult>(this CompositeStrategyBuilder<TResult> builder, TimeSpan timeout)
    {
        Guard.NotNull(builder);

        return builder.AddTimeout(new TimeoutStrategyOptions
        {
            Timeout = timeout
        });
    }

    /// <summary>
    /// Adds a timeout resilience strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="timeout">The timeout value. This value should be greater than <see cref="TimeSpan.Zero"/>.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the options produced from the arguments are invalid.</exception>
    public static CompositeStrategyBuilder AddTimeout(this CompositeStrategyBuilder builder, TimeSpan timeout)
    {
        Guard.NotNull(builder);

        return builder.AddTimeout(new TimeoutStrategyOptions
        {
            Timeout = timeout
        });
    }

    /// <summary>
    /// Adds a timeout resilience strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The timeout options.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TimeoutStrategyOptions))]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static CompositeStrategyBuilder AddTimeout(this CompositeStrategyBuilder builder, TimeoutStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddStrategy(context => new TimeoutResilienceStrategy<object>(options, context.TimeProvider, context.Telemetry), options);
        return builder;
    }

    /// <summary>
    /// Adds a timeout resilience strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The timeout options.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TimeoutStrategyOptions))]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static CompositeStrategyBuilder<TResult> AddTimeout<TResult>(this CompositeStrategyBuilder<TResult> builder, TimeoutStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddStrategy(context => new TimeoutResilienceStrategy<TResult>(options, context.TimeProvider, context.Telemetry), options);
        return builder;
    }
}
