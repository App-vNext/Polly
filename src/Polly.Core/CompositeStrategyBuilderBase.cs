using System.ComponentModel;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Telemetry;

namespace Polly;

/// <summary>
/// A builder that is used to create an instance of <see cref="ResilienceStrategy"/>.
/// </summary>
/// <remarks>
/// The builder supports combining multiple strategies into a composite resilience strategy.
/// The resulting instance of <see cref="ResilienceStrategy"/> executes the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
public abstract class CompositeStrategyBuilderBase
{
    private readonly List<Entry> _entries = new();
    private bool _used;

    private protected CompositeStrategyBuilderBase()
    {
    }

    private protected CompositeStrategyBuilderBase(CompositeStrategyBuilderBase other)
    {
        Name = other.Name;
        TimeProvider = other.TimeProvider;
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
    internal void AddStrategyCore(Func<StrategyBuilderContext, ResilienceStrategy> factory, ResilienceStrategyOptions options)
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

    internal ResilienceStrategy BuildStrategy()
    {
        Validator(new(this, $"The '{nameof(CompositeStrategyBuilder)}' configuration is invalid."));

        _used = true;

        var strategies = _entries.Select(CreateResilienceStrategy).ToList();

        if (strategies.Count == 0)
        {
            return NullResilienceStrategy.Instance;
        }

        return CompositeResilienceStrategy.Create(
            strategies,
            TelemetryUtil.CreateTelemetry(DiagnosticSource, Name, InstanceName, null),
            TimeProvider);
    }

    private ResilienceStrategy CreateResilienceStrategy(Entry entry)
    {
        var context = new StrategyBuilderContext(
            builderName: Name,
            builderInstanceName: InstanceName,
            strategyName: entry.Options.Name,
            timeProvider: TimeProvider,
            diagnosticSource: DiagnosticSource);

        var strategy = entry.Factory(context);
        strategy.Options = entry.Options;
        return strategy;
    }

    private sealed record Entry(Func<StrategyBuilderContext, ResilienceStrategy> Factory, ResilienceStrategyOptions Options);
}
