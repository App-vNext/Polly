using System.ComponentModel;

using System.ComponentModel.DataAnnotations;

namespace Polly;

/// <summary>
/// A builder that is used to create an instance of <see cref="ResilienceStrategy"/>.
/// </summary>
/// <remarks>
/// The builder supports chaining multiple strategies into a pipeline of strategies.
/// The resulting instance of <see cref="ResilienceStrategy"/> executes the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
public abstract class ResilienceStrategyBuilderBase
{
    private readonly List<Entry> _entries = new();
    private bool _used;

    private protected ResilienceStrategyBuilderBase()
    {
    }

    private protected ResilienceStrategyBuilderBase(ResilienceStrategyBuilderBase other)
    {
        BuilderName = other.BuilderName;
        Properties = other.Properties;
        TimeProvider = other.TimeProvider;
        OnCreatingStrategy = other.OnCreatingStrategy;
        Randomizer = other.Randomizer;
        DiagnosticSource = other.DiagnosticSource;
    }

    /// <summary>
    /// Gets or sets the name of the builder.
    /// </summary>
    /// <remarks>
    /// This property is also included in the telemetry that is produced by the individual resilience strategies.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    public string? BuilderName { get; set; }

    /// <summary>
    /// Gets or sets the instance name of the builder.
    /// </summary>
    /// <remarks>
    /// This property is also included in the telemetry that is produced by the individual resilience strategies.
    /// The instance name can be used to differentiate between multiple builder instances with the same <see cref="BuilderName"/>.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    public string? InstanceName { get; set; }

    /// <summary>
    /// Gets the custom properties attached to builder options.
    /// </summary>
    /// <value>
    /// The default value is the empty instance of <see cref="ResilienceProperties"/>.
    /// </value>
    public ResilienceProperties Properties { get; } = new();

    /// <summary>
    /// Gets or sets a <see cref="TimeProvider"/> that is used by strategies that work with time.
    /// </summary>
    /// <remarks>
    /// This property is internal until we switch to official System.TimeProvider.
    /// </remarks>
    /// <value>
    /// The default value is <see cref="TimeProvider.System"/>.
    /// </value>
    [Required]
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <summary>
    /// Gets or sets the callback that is invoked just before the final resilience strategy is being created.
    /// </summary>
    /// <remarks>
    /// This property is used by the telemetry infrastructure and should not be used directly by user code.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Action<IList<ResilienceStrategy>>? OnCreatingStrategy { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DiagnosticSource"/> that is used by Polly to report resilience events.
    /// </summary>
    /// <remarks>
    /// This property is used by the telemetry infrastructure and should not be used directly by user code.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DiagnosticSource? DiagnosticSource { get; set; }

    /// <summary>
    /// Gets or sets the randomizer that is used by strategies that need to generate random numbers.
    /// </summary>
    /// <value>
    /// The default value is thread safe randomizer function that returns values between 0.0 and 1.0.
    /// </value>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Required]
    public Func<double> Randomizer { get; set; } = RandomUtil.Instance.NextDouble;

    internal abstract bool IsGenericBuilder { get; }

    internal void AddStrategyCore(Func<ResilienceStrategyBuilderContext, ResilienceStrategy> factory, ResilienceStrategyOptions options)
    {
        Guard.NotNull(factory);
        Guard.NotNull(options);

        ValidationHelper.ValidateObject(options, $"The '{TypeNameFormatter.Format(options.GetType())}' are invalid.");

        if (_used)
        {
            throw new InvalidOperationException("Cannot add any more resilience strategies to the builder after it has been used to build a strategy once.");
        }

        _entries.Add(new Entry(factory, options));
    }

    internal ResilienceStrategy BuildStrategy()
    {
        ValidationHelper.ValidateObject(this, $"The '{nameof(ResilienceStrategyBuilder)}' configuration is invalid.");

        _used = true;

        var strategies = _entries.Select(CreateResilienceStrategy).ToList();
        OnCreatingStrategy?.Invoke(strategies);

        if (strategies.Count == 0)
        {
            return NullResilienceStrategy.Instance;
        }

        if (strategies.Count == 1)
        {
            return strategies[0];
        }

        return ResilienceStrategyPipeline.CreatePipeline(strategies);
    }

    private ResilienceStrategy CreateResilienceStrategy(Entry entry)
    {
        var context = new ResilienceStrategyBuilderContext(
            builderName: BuilderName,
            builderInstanceName: InstanceName,
            builderProperties: Properties,
            strategyName: entry.Options.StrategyName,
            strategyType: entry.Options.StrategyType,
            timeProvider: TimeProvider,
            isGenericBuilder: IsGenericBuilder,
            diagnosticSource: DiagnosticSource,
            randomizer: Randomizer);

        var strategy = entry.Factory(context);
        strategy.Options = entry.Options;
        return strategy;
    }

    private sealed record Entry(Func<ResilienceStrategyBuilderContext, ResilienceStrategy> Factory, ResilienceStrategyOptions Options);
}
