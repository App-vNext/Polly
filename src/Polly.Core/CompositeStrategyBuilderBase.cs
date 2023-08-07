using System.ComponentModel;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Polly;

/// <summary>
/// A builder that is used to create an instance of <see cref="ResilienceStrategy"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <remarks>
/// The builder supports combining multiple strategies into a composite resilience strategy.
/// The resulting instance of <see cref="ResilienceStrategy"/> executes the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
public abstract class CompositeStrategyBuilderBase<TResult>
{
    private readonly List<Entry> _entries = new();
    private bool _used;

    private protected CompositeStrategyBuilderBase()
    {
    }

    private protected CompositeStrategyBuilderBase(CompositeStrategyBuilderBase<object> other)
    {
        Name = other.Name;
        Properties = other.Properties;
        TimeProvider = other.TimeProvider;
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
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the instance name of the builder.
    /// </summary>
    /// <remarks>
    /// This property is also included in the telemetry that is produced by the individual resilience strategies.
    /// The instance name can be used to differentiate between multiple builder instances with the same <see cref="Name"/>.
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
    public Action<IList<ResilienceStrategy<TResult>>>? OnCreatingStrategy { get; set; }

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
    /// The default value is thread-safe randomizer that returns values between 0.0 and 1.0.
    /// </value>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Required]
    public Func<double> Randomizer { get; set; } = RandomUtil.Instance.NextDouble;

    /// <summary>
    /// Gets the validator that is used for the validation.
    /// </summary>
    /// <value>The default value is a validation function that uses data annotations for validation.</value>
    /// <remarks>
    /// The validator should throw <see cref="ValidationException"/> when the validated instance is invalid.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the attempting to assign <see langword="null"/> to this property.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Action<ResilienceValidationContext> Validator { get; private protected set; } = ValidationHelper.ValidateObject;

    [RequiresUnreferencedCode(Constants.OptionsValidation)]
    internal void AddStrategyCore(Func<StrategyBuilderContext, ResilienceStrategy<TResult>> factory, ResilienceStrategyOptions options)
    {
        Guard.NotNull(factory);
        Guard.NotNull(options);

        Validator(new ResilienceValidationContext(options, $"The '{TypeNameFormatter.Format(options.GetType())}' are invalid."));

        if (_used)
        {
            throw new InvalidOperationException("Cannot add any more resilience strategies to the builder after it has been used to build a strategy once.");
        }

        _entries.Add(new Entry(factory, options));
    }

    internal ResilienceStrategy<TResult> BuildStrategy()
    {
        Validator(new(this, $"The '{nameof(CompositeStrategyBuilder)}' configuration is invalid."));

        _used = true;

        var strategies = _entries.Select(CreateResilienceStrategy).ToList();
        OnCreatingStrategy?.Invoke(strategies);

        if (strategies.Count == 0)
        {
            return NullResilienceStrategy<TResult>.Instance;
        }

        if (strategies.Count == 1)
        {
            return strategies[0];
        }

        return CompositeResilienceStrategy<TResult>.Create(strategies);
    }

    private ResilienceStrategy<TResult> CreateResilienceStrategy(Entry entry)
    {
        var context = new StrategyBuilderContext(
            builderName: Name,
            builderInstanceName: InstanceName,
            builderProperties: Properties,
            strategyName: entry.Options.Name,
            timeProvider: TimeProvider,
            diagnosticSource: DiagnosticSource,
            randomizer: Randomizer);

        var strategy = entry.Factory(context);
        strategy.Options = entry.Options;
        return strategy;
    }

    private sealed record Entry(Func<StrategyBuilderContext, ResilienceStrategy<TResult>> Factory, ResilienceStrategyOptions Options);
}
