using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Polly.Extensions.Telemetry;

/// <summary>
/// The options that are used to configure the telemetry that is produced by the resilience strategies.
/// </summary>
public class TelemetryOptions
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
    /// Gets or sets the result formatter.
    /// </summary>
    /// <remarks>
    /// Defaults to a formatter that returns a status code for HTTP based responses and the result as-is for all other result types.
    /// <para>
    /// This property is required.
    /// </para>
    /// </remarks>
    [Required]
    public Func<ResilienceContext, object?, object?> ResultFormatter { get; set; } = (_, result) => result switch
    {
        HttpResponseMessage response => (int)response.StatusCode,
        _ => result,
    };
}
