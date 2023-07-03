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
    /// <param name="fault">The exception to inject.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder AddFault(this ResilienceStrategyBuilder builder, Exception fault)
    {
        Guard.NotNull(builder);

        return builder.AddOutcomeCore(new OutcomeStrategyOptions<Exception>
        {
            Outcome = new(fault)
        });
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="faultGenerator">The exception generator delegate.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder AddFault(this ResilienceStrategyBuilder builder, Func<ValueTask<Outcome<Exception>>> faultGenerator)
    {
        Guard.NotNull(builder);

        return builder.AddOutcomeCore(new OutcomeStrategyOptions<Exception>
        {
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
    /// <param name="result">The outcome to inject.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder<TResult> AddResult<TResult>(this ResilienceStrategyBuilder<TResult> builder, TResult result)
    {
        Guard.NotNull(builder);

        return builder.AddOutcomeCore(new OutcomeStrategyOptions<TResult>
        {
            Outcome = new(result)
        });
    }

    /// <summary>
    /// Adds an outcome chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="outcomeGenerator">The outcome generator delegate.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static ResilienceStrategyBuilder<TResult> AddResult<TResult>(this ResilienceStrategyBuilder<TResult> builder, Func<ValueTask<Outcome<TResult>>> outcomeGenerator)
    {
        Guard.NotNull(builder);

        return builder.AddOutcomeCore(new OutcomeStrategyOptions<TResult>
        {
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
