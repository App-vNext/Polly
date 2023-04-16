using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Strategy;

namespace Polly.Extensions.Telemetry;

/// <summary>
/// The options that are used to configure the telemetry that is produced by the resilience strategies.
/// </summary>
public class ResilienceStrategyTelemetryOptions
{
    /// <summary>
    /// Gets or sets the logger factory.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="NullLoggerFactory.Instance"/>.
    /// </remarks>
    [Required]
    public ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;

    /// <summary>
    /// Gets the registered resilience telemetry enrichers.
    /// </summary>
    /// <remarks>
    /// Defaults to an empty collection.
    /// </remarks>
    public ICollection<Action<EnrichmentContext>> Enrichers { get; } = new List<Action<EnrichmentContext>>();

    /// <summary>
    /// Gets or sets the formatter that converts the outcome to a string.
    /// </summary>
    /// <remarks>
    /// Defaults to formatter that calls <see cref="Outcome.ToString"/> when formatting the outcome.
    /// </remarks>
    [Required]
    public Func<Outcome, string> OutcomeFormatter { get; set; } = outcome => outcome.ToString();
}
