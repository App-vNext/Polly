using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Polly.Telemetry;
using Polly.Utils.Pipeline;

namespace Polly;

/// <summary>
/// A builder that is used to create an instance of <see cref="ResiliencePipeline"/>.
/// </summary>
/// <remarks>
/// The builder supports combining multiple strategies into a pipeline of resilience strategies.
/// The resulting instance of <see cref="ResiliencePipeline"/> executes the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
#pragma warning disable S1694 // An abstract class should have both abstract and concrete methods
public abstract class ResiliencePipelineBuilderBase
#pragma warning restore S1694 // An abstract class should have both abstract and concrete methods
{
    private readonly List<Entry> _entries = [];
    private bool _used;

    private protected ResiliencePipelineBuilderBase()
    {
    }

    private protected ResiliencePipelineBuilderBase(ResiliencePipelineBuilderBase other)
    {
        Name = other.Name;
        TimeProvider = other.TimeProvider;
        TelemetryListener = other.TelemetryListener;
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
    /// Gets or sets the <see cref="ResilienceContextPool"/> associated with the builder.
    /// </summary>
    /// <remarks>
    /// A custom pool can be used to configure custom behavior for  creation.
    /// This can include setting a default <c>continueOnCapturedContext</c> parameter or custom operation key resolution.
    /// </remarks>
    /// <value>
    /// If the default value of <see langword="null"/> is used, <see cref="ResilienceContextPool.Shared"/> will be used.
    /// </value>
    public ResilienceContextPool? ContextPool { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="TimeProvider"/> that is used by strategies that work with time.
    /// </summary>
    /// <value>
    /// The default value is <see langword="null"/> and unless set, <see cref="TimeProvider.System"/> will be used.
    /// </value>
    public TimeProvider? TimeProvider { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Telemetry.TelemetryListener"/> that is used by Polly to report resilience events.
    /// </summary>
    /// <remarks>
    /// This property is used by the telemetry infrastructure and should not be used directly by user code.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public TelemetryListener? TelemetryListener { get; set; }

    internal TimeProvider TimeProviderInternal => TimeProvider ?? TimeProvider.System;

    /// <summary>
    /// Gets the validator that is used for the validation.
    /// </summary>
    /// <value>The default value is a validation function that uses data annotations for validation.</value>
    /// <remarks>
    /// The validator should throw <see cref="ValidationException"/> when the validated instance is invalid.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the attempting to assign <see langword="null"/> to this property.</exception>
    internal Action<ResilienceValidationContext> Validator { get; private protected set; } = ValidationHelper.ValidateObject;

    [RequiresUnreferencedCode(Constants.OptionsValidation)]
    internal void AddPipelineComponent(Func<StrategyBuilderContext, PipelineComponent> factory, ResilienceStrategyOptions options)
    {
        Guard.NotNull(factory);
        Guard.NotNull(options);

        Validator(new ResilienceValidationContext(options, $"The '{TypeNameFormatter.Format(options.GetType())}' are invalid."));

        if (_used)
        {
            throw new InvalidOperationException("Cannot add any more resilience strategies to the builder after it has been used to build a pipeline once.");
        }

        _entries.Add(new Entry(factory, options));
    }

    internal PipelineComponent BuildPipelineComponent()
    {
        Validator(new(this, $"The '{nameof(ResiliencePipelineBuilder)}' configuration is invalid."));

        _used = true;

        var components = _entries.ConvertAll(CreateComponent);

        if (components.Count == 0)
        {
            return PipelineComponent.Empty;
        }

        var source = new ResilienceTelemetrySource(Name, InstanceName, null);

        return PipelineComponentFactory.CreateComposite(components, new ResilienceStrategyTelemetry(source, TelemetryListener), TimeProviderInternal);
    }

    private PipelineComponent CreateComponent(Entry entry)
    {
        var source = new ResilienceTelemetrySource(Name, InstanceName, entry.Options.Name);
        var context = new StrategyBuilderContext(new ResilienceStrategyTelemetry(source, TelemetryListener), TimeProviderInternal);

        var strategy = entry.Factory(context);
        strategy.Options = entry.Options;
        return strategy;
    }

    private sealed record Entry(Func<StrategyBuilderContext, PipelineComponent> Factory, ResilienceStrategyOptions Options);
}
