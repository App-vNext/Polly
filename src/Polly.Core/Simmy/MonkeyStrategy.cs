using Polly.Simmy.Utils;

namespace Polly.Simmy;

#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable S3928 // Custom ArgumentNullException message

/// <summary>
/// Contains common functionality for chaos strategies which intentionally disrupt executions - which monkey around with calls.
/// </summary>
public abstract class MonkeyStrategy : ResilienceStrategy
{
    private readonly Func<double> _randomizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonkeyStrategy"/> class.
    /// </summary>
    /// <param name="options">The chaos strategy options.</param>
    protected MonkeyStrategy(MonkeyStrategyOptions options)
    {
        Guard.NotNull(options);
        Guard.NotNull(options.Randomizer);

        _randomizer = options.Randomizer;
        InjectionRateGenerator = options.InjectionRateGenerator is not null ? options.InjectionRateGenerator : (_) => new(options.InjectionRate);
        EnabledGenerator = options.EnabledGenerator is not null ? options.EnabledGenerator : (_) => new(options.Enabled);
    }

    /// <summary>
    /// Gets the injection rate for a given execution, which the value should be between [0, 1].
    /// </summary>
    public Func<InjectionRateGeneratorArguments, ValueTask<double>> InjectionRateGenerator { get; }

    /// <summary>
    /// Gets a value that indicates whether or not the chaos strategy is enabled for a given execution.
    /// </summary>
    public Func<EnabledGeneratorArguments, ValueTask<bool>> EnabledGenerator { get; }

    /// <summary>
    /// Determines whether or not the chaos strategy should be injected based on the injection rate and enabled flag.
    /// </summary>
    /// <param name="context">The <see cref="ResilienceContext"/> instance.</param>
    /// <returns>A boolean value that indicates whether or not the chaos strategy should be injected.</returns>
    /// <remarks>Use this method before injecting any chaos strategy to evaluate whether a given chaos strategy needs to be injected during the execution.</remarks>
    public async ValueTask<bool> ShouldInjectAsync(ResilienceContext context)
    {
        Guard.NotNull(context);

        // to prevent executing config delegates if token was signaled before to start.
        context.CancellationToken.ThrowIfCancellationRequested();

        if (!await EnabledGenerator(new EnabledGeneratorArguments { Context = context }).ConfigureAwait(context.ContinueOnCapturedContext))
        {
            return false;
        }

        // to prevent executing InjectionRate config delegate if token was signaled on Enabled configuration delegate.
        context.CancellationToken.ThrowIfCancellationRequested();

        double injectionThreshold = await InjectionRateGenerator(new InjectionRateGeneratorArguments { Context = context })
            .ConfigureAwait(context.ContinueOnCapturedContext);

        // to prevent executing further config delegates if token was signaled on InjectionRate configuration delegate.
        context.CancellationToken.ThrowIfCancellationRequested();

        injectionThreshold.EnsureInjectionThreshold();
        return _randomizer() < injectionThreshold;
    }
}
