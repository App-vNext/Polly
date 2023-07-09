namespace Polly.Simmy.Outcomes;

/// <summary>
/// Extension methods for adding outcome to a <see cref="ResilienceStrategyBuilder"/>.
/// </summary>
public static class OutcomeChaosStrategyBuilderExtensions
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

        return builder.AddOutcomeCore(new OutcomeStrategyOptions<Exception>
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
    public static ResilienceStrategyBuilder AddFault(this ResilienceStrategyBuilder builder, bool enabled, double injectionRate, Func<ValueTask<Outcome<Exception>>> faultGenerator)
    {
        Guard.NotNull(builder);

        return builder.AddOutcomeCore(new OutcomeStrategyOptions<Exception>
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

        return builder.AddOutcomeCore(options);
    }

    /// <summary>
    /// Adds an outcome chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1].</param>
    /// <param name="result">The outcome to inject.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder<TResult> AddResult<TResult>(this ResilienceStrategyBuilder<TResult> builder, bool enabled, double injectionRate, TResult result)
    {
        Guard.NotNull(builder);

        return builder.AddOutcomeCore(new OutcomeStrategyOptions<TResult>
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            Outcome = new(result)
        });
    }

    /// <summary>
    /// Adds an outcome chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1].</param>
    /// <param name="outcomeGenerator">The outcome generator delegate.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder<TResult> AddResult<TResult>(
        this ResilienceStrategyBuilder<TResult> builder, bool enabled, double injectionRate, Func<ValueTask<Outcome<TResult>>> outcomeGenerator)
    {
        Guard.NotNull(builder);

        return builder.AddOutcomeCore(new OutcomeStrategyOptions<TResult>
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            OutcomeGenerator = (_) => outcomeGenerator()
        });
    }

    /// <summary>
    /// Adds an outcome chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The outcome strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder<TResult> AddResult<TResult>(this ResilienceStrategyBuilder<TResult> builder, OutcomeStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddOutcomeCore(options);
    }

    private static TBuilder AddOutcomeCore<TBuilder, TResult>(this TBuilder builder, OutcomeStrategyOptions<TResult> options)
        where TBuilder : ResilienceStrategyBuilderBase
    {
        return builder.AddStrategy(context =>
            new OutcomeChaosStrategy<TResult>(
                options,
                context.Telemetry),
            options);
    }
}
