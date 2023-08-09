// Assembly 'Polly.Extensions'

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Polly.Telemetry;

public class TelemetryOptions
{
    public Action<TelemetryEventArguments>? OnTelemetryEvent { get; set; }
    [Required]
    public ILoggerFactory LoggerFactory { get; set; }
    public ICollection<Action<EnrichmentContext>> Enrichers { get; }
    [Required]
    public Func<ResilienceContext, object?, object?> ResultFormatter { get; set; }
    public TelemetryOptions();
}
