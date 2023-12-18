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
    public ICollection<TelemetryListener> TelemetryListeners { get; } = new List<TelemetryListener>();

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
    public ICollection<MeteringEnricher> MeteringEnrichers { get; } = new List<MeteringEnricher>();

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

    /// <summary>
    /// Gets or sets the resilience event severity provider.
    /// </summary>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    public Func<ResilienceEvent, ResilienceEventSeverity>? ResilienceEventSeverityProvider { get; set; }

    internal static TelemetryOptions Combine(params TelemetryOptions[] options)
    {
        var result = new TelemetryOptions
        {
            LoggerFactory = options
                .Select(v => v.LoggerFactory)
                .FirstOrDefault(v => v is not null)!,
            ResultFormatter = options
                .Select(v => v.ResultFormatter)
                .FirstOrDefault(v => v is not null)!,
            ResilienceEventSeverityProvider = options
                .Select(v => v.ResilienceEventSeverityProvider)
                .FirstOrDefault(v => v is not null),
        };

        foreach (TelemetryOptions o in options)
        {
            ((List<TelemetryListener>)result.TelemetryListeners).AddRange(o.TelemetryListeners);
            ((List<MeteringEnricher>)result.MeteringEnrichers).AddRange(o.MeteringEnrichers);
        }

        return result;
    }
}
