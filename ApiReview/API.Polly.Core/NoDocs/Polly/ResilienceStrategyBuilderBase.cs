// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Polly;

public abstract class ResilienceStrategyBuilderBase
{
    public string? BuilderName { get; set; }
    public string? InstanceName { get; set; }
    public ResilienceProperties Properties { get; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Action<IList<ResilienceStrategy>>? OnCreatingStrategy { get; set; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DiagnosticSource? DiagnosticSource { get; set; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Required]
    public Func<double> Randomizer { get; set; }
}
