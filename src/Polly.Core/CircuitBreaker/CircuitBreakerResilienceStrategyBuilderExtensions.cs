using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.CircuitBreaker;
using Polly.CircuitBreaker.Health;

namespace Polly;

/// <summary>
/// Circuit breaker strategy extensions for <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class CircuitBreakerResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Add circuit breaker strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>A builder with the circuit breaker strategy added.</returns>
    /// <remarks>
    /// See <see cref="CircuitBreakerStrategyOptions{TResult}"/> for more details about the circuit breaker strategy.
    /// <para>
    /// If you are discarding the strategy created by this call make sure to use <see cref="CircuitBreakerManualControl"/> and dispose the manual control instance when the strategy is no longer used.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static ResilienceStrategyBuilder AddCircuitBreaker(this ResilienceStrategyBuilder builder, CircuitBreakerStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddCircuitBreakerCore(options);
    }

    /// <summary>
    /// Add circuit breaker strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the circuit breaker strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>A builder with the circuit breaker strategy added.</returns>
    /// <remarks>
    /// See <see cref="CircuitBreakerStrategyOptions{TResult}"/> for more details about the circuit breaker strategy.
    /// <para>
    /// If you are discarding the strategy created by this call make sure to use <see cref="CircuitBreakerManualControl"/> and dispose the manual control instance when the strategy is no longer used.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="options"/> are invalid.</exception>
    public static ResilienceStrategyBuilder<TResult> AddCircuitBreaker<TResult>(this ResilienceStrategyBuilder<TResult> builder, CircuitBreakerStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddCircuitBreakerCore(options);
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    private static TBuilder AddCircuitBreakerCore<TBuilder, TResult>(this TBuilder builder, CircuitBreakerStrategyOptions<TResult> options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        return builder.AddStrategy(
            context =>
            {
                var behavior = new AdvancedCircuitBehavior(
                    options.FailureRatio,
                    options.MinimumThroughput,
                    HealthMetrics.Create(options.SamplingDuration, context.TimeProvider));

                return CreateStrategy<TResult, CircuitBreakerStrategyOptions<TResult>>(context, options, behavior);
            },
            options);
    }

    internal static CircuitBreakerResilienceStrategy<TResult> CreateStrategy<TResult, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        ResilienceStrategyBuilderContext context,
        CircuitBreakerStrategyOptions<TResult> options,
        CircuitBehavior behavior)
    {
        var controller = new CircuitStateController<TResult>(
            options.BreakDuration,
            options.OnOpened,
            options.OnClosed,
            options.OnHalfOpened,
            behavior,
            context.TimeProvider,
            context.Telemetry);

        return new CircuitBreakerResilienceStrategy<TResult>(
            options.ShouldHandle!,
            controller,
            options.StateProvider,
            options.ManualControl);
    }
}

