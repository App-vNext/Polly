using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Retry;

namespace Polly;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

/// <summary>
/// Retry extension methods for the <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class RetryResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a retry strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The retry strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static ResilienceStrategyBuilder AddRetry(this ResilienceStrategyBuilder builder, RetryStrategyOptions options)
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
    public static ResilienceStrategyBuilder<TResult> AddRetry<TResult>(this ResilienceStrategyBuilder<TResult> builder, RetryStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddRetryCore<TResult, RetryStrategyOptions<TResult>>(options);
        return builder;
    }

    private static void AddRetryCore<TResult, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        this ResilienceStrategyBuilderBase builder,
        RetryStrategyOptions<TResult> options)
    {
        builder.AddStrategy(context =>
            new RetryResilienceStrategy<TResult>(
                options,
                context.IsGenericBuilder,
                context.TimeProvider,
                context.Telemetry,
                context.Randomizer),
            options);
    }
}
