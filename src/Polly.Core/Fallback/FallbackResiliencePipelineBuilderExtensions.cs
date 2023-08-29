using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Fallback;

namespace Polly;

/// <summary>
/// Extensions for adding fallback to <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class FallbackResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// Adds a fallback resilience strategy with the provided options to the builder.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="builder">The resilience pipeline builder.</param>
    /// <param name="options">The options to configure the fallback resilience strategy.</param>
    /// <returns>The builder instance with the fallback strategy added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    public static ResiliencePipelineBuilder<TResult> AddFallback<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        FallbackStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(context => CreateFallback(context, options), options);
    }

    private static ResilienceStrategy<TResult> CreateFallback<TResult>(
        StrategyBuilderContext context,
        FallbackStrategyOptions<TResult> options)
    {
        var handler = new FallbackHandler<TResult>(
            options.ShouldHandle!,
            options.FallbackAction!);

        return new FallbackResilienceStrategy<TResult>(
            handler,
            options.OnFallback,
            context.Telemetry);
    }
}
