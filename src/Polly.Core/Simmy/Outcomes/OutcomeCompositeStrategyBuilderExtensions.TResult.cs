using System.Diagnostics.CodeAnalysis;

namespace Polly.Simmy.Outcomes;

/// <summary>
/// Extension methods for adding outcome to a <see cref="CompositeStrategyBuilder"/>.
/// </summary>
public static partial class OutcomeCompositeStrategyBuilderExtensions
{
    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1].</param>
    /// <param name="fault">The exception to inject.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static CompositeStrategyBuilder<TResult> AddFault<TResult>(this CompositeStrategyBuilder<TResult> builder, bool enabled, double injectionRate, Exception fault)
    {
        Guard.NotNull(builder);

        return builder.AddFaultCore<CompositeStrategyBuilder<TResult>, TResult>(new OutcomeStrategyOptions<Exception>
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            Outcome = new(fault)
        });
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="enabled">A value that indicates whether or not the chaos strategy is enabled for a given execution.</param>
    /// <param name="injectionRate">The injection rate for a given execution, which the value should be between [0, 1].</param>
    /// <param name="faultGenerator">The exception generator delegate.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static CompositeStrategyBuilder<TResult> AddFault<TResult>(
        this CompositeStrategyBuilder<TResult> builder, bool enabled, double injectionRate, Func<ValueTask<Outcome<Exception>>> faultGenerator)
    {
        Guard.NotNull(builder);

        return builder.AddFaultCore<CompositeStrategyBuilder<TResult>, TResult>(new OutcomeStrategyOptions<Exception>
        {
            Enabled = enabled,
            InjectionRate = injectionRate,
            OutcomeGenerator = (_) => faultGenerator()
        });
    }

    /// <summary>
    /// Adds a fault chaos strategy to the builder.
    /// </summary>
    /// <typeparam name="TResult">The type of result the retry strategy handles.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The fault strategy options.</param>
    /// <returns>The builder instance with the retry strategy added.</returns>
    public static CompositeStrategyBuilder<TResult> AddFault<TResult>(this CompositeStrategyBuilder<TResult> builder, OutcomeStrategyOptions<Exception> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddFaultCore<CompositeStrategyBuilder<TResult>, TResult>(options);
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
    public static CompositeStrategyBuilder<TResult> AddResult<TResult>(this CompositeStrategyBuilder<TResult> builder, bool enabled, double injectionRate, TResult result)
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
    public static CompositeStrategyBuilder<TResult> AddResult<TResult>(
        this CompositeStrategyBuilder<TResult> builder, bool enabled, double injectionRate, Func<ValueTask<Outcome<TResult>>> outcomeGenerator)
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
    public static CompositeStrategyBuilder<TResult> AddResult<TResult>(this CompositeStrategyBuilder<TResult> builder, OutcomeStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddOutcomeCore(options);
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    private static TBuilder AddOutcomeCore<TBuilder, TResult>(this TBuilder builder, OutcomeStrategyOptions<TResult> options)
        where TBuilder : CompositeStrategyBuilderBase
    {
        return builder.AddStrategy(context =>
            new OutcomeChaosStrategy<TResult>(
                options,
                context.Telemetry),
            options);
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "All options members preserved.")]
    private static TBuilder AddFaultCore<TBuilder, TResult>(this TBuilder builder, OutcomeStrategyOptions<Exception> options)
        where TBuilder : CompositeStrategyBuilderBase
    {
        return builder.AddStrategy(context =>
            new OutcomeChaosStrategy<TResult>(
                options,
                context.Telemetry),
            options);
    }
}
