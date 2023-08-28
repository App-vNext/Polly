// Assembly 'Polly.Extensions'

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Polly.Telemetry;

public class TelemetryOptions
{
    public ICollection<TelemetryListener> TelemetryListeners { get; }
    [Required]
    public ILoggerFactory LoggerFactory { get; set; }
    public ICollection<MeteringEnricher> MeteringEnrichers { get; }
    [Required]
    public Func<ResilienceContext, object?, object?> ResultFormatter { get; set; }
    public TelemetryOptions();
}
