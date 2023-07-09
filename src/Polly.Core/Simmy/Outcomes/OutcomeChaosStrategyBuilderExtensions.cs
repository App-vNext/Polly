namespace Polly.Simmy.Outcomes;

/// <summary>
/// Extension methods for adding outcome to a <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static partial class OutcomeChaosStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1].</param>
    /// <param name="fault">The exception to inject.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder AddFault(this ResilienceStrategyBuilder builder, bool enabled, double injectionRate, Exception fault)
    {
        Guard.NotNull(builder);

        return builder.AddFaultCore(new OutcomeStrategyOptions<Exception>
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            Outcome = new(fault)
        });
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1].</param>
    /// <param name="faultGenerator">The exception generator delegate.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder AddFault(
        this ResilienceStrategyBuilder builder, bool enabled, double injectionRate, Func<ValueTask<Outcome<Exception>>> faultGenerator)
    {
        Guard.NotNull(builder);

        return builder.AddFaultCore(new OutcomeStrategyOptions<Exception>
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            OutcomeGenerator = (_) => faultGenerator()
        });
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The fault strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder AddFault(this ResilienceStrategyBuilder builder, OutcomeStrategyOptions<Exception> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddFaultCore(options);
    }

    private static TBuilder AddFaultCore<TBuilder>(this TBuilder builder, OutcomeStrategyOptions<Exception> options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        return builder.AddStrategy(context =>
            new OutcomeChaosStrategy(
                options,
                context.Telemetry,
                context.IsGenericBuilder),
            options);
    }
}
