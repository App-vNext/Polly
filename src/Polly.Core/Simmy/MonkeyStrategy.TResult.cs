﻿using Polly.Simmy.Utils;

namespace Polly.Simmy;

/// <summary>
/// This base strategy class is used to simplify the implementation of generic (reactive)
/// strategies by limiting the number of generic types the execute method receives.
/// </summary>
/// <typeparam name="T">The type of result this strategy handles.</typeparam>
/// <remarks>
/// For strategies that handle all result types the generic parameter must be of type <see cref="object"/>.
/// </remarks>
public abstract class MonkeyStrategy<T> : ResilienceStrategy<T>
{
    private readonly Func<double> _randomizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonkeyStrategy{T}"/> class.
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
    /// Gets the injection rate for a given execution, which the value should be between [0, 1] (inclusive).
    /// </summary>
    internal Func<InjectionRateGeneratorArguments, ValueTask<double>> InjectionRateGenerator { get; }

    /// <summary>
    /// Gets a value that indicates whether or not the chaos strategy is enabled for a given execution.
    /// </summary>
    internal Func<EnabledGeneratorArguments, ValueTask<bool>> EnabledGenerator { get; }

    /// <summary>
    /// Determines whether or not the chaos strategy should be injected based on the injection rate and enabled flag.
    /// </summary>
    /// <param name="context">The <see cref="ResilienceContext"/> instance.</param>
    /// <returns>A boolean value that indicates whether or not the chaos strategy should be injected.</returns>
    /// <remarks>Use this method before injecting any chaos strategy to evaluate whether a given chaos strategy needs to be injected during the execution.</remarks>
    protected async ValueTask<bool> ShouldInjectAsync(ResilienceContext context)
    {
        return await MonkeyStrategyHelper
            .ShouldInjectAsync(context, InjectionRateGenerator, EnabledGenerator, _randomizer)
            .ConfigureAwait(false);
    }
}
