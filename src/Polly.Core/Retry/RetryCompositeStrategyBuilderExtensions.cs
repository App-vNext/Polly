using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Retry;

namespace Polly;

/// <summary>
/// Retry extension methods for the <see cref="CompositeStrategyBuilder"/>.
/// </summary>
public static class RetryCompositeStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The retry strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static CompositeStrategyBuilder AddRetry(this CompositeStrategyBuilder builder, RetryStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddRetryCore<object, RetryStrategyOptions>(options);
        return builder;
    }

    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The retry strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static CompositeStrategyBuilder<TResult> AddRetry<TResult>(this CompositeStrategyBuilder<TResult> builder, RetryStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddRetryCore<TResult, RetryStrategyOptions<TResult>>(options);
        return builder;
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    private static void AddRetryCore<TResult, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        this CompositeStrategyBuilderBase<TResult> builder,
        RetryStrategyOptions<TResult> options)
    {
        builder.AddStrategyCore(context =>
            new RetryResilienceStrategy<TResult>(
                options,
                context.TimeProvider,
                context.Telemetry,
                context.Randomizer),
            options);
    }
}
