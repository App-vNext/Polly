using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Polly.Telemetry;

/// <summary>
/// The options that are used to configure the telemetry that is produced by the resilience strategies.
/// </summary>
public class TelemetryOptions
{
    /// <summary>
    /// Gets the collection of telemetry listeners.
    /// </summary>
    /// <value>
    /// The default value is an empty collection.
    /// </value>
    public ICollection<TelemetryListener> TelemetryListeners { get; } = [];

    /// <summary>
    /// Gets or sets the logger factory.
    /// </summary>
    /// <value>
    /// The default value is <see cref="NullLoggerFactory.Instance"/>.
    /// </value>
    [Required]
    public ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;

    /// <summary>
    /// Gets the collection of telemetry enrichers.
    /// </summary>
    /// <value>
    /// The default value is an empty collection.
    /// </value>
    public ICollection<MeteringEnricher> MeteringEnrichers { get; } = [];

    /// <summary>
    /// Gets or sets the result formatter.
    /// </summary>
    /// <value>
    /// The default value is a formatter that returns a status code for HTTP based responses and the result as-is for all other result types.
    /// This property is required.
    /// </value>
    [Required]
    public Func<ResilienceContext, object?, object?> ResultFormatter { get; set; } = (_, result) => result switch
    {
        HttpResponseMessage response => (int)response.StatusCode,
        _ => result,
    };
}
