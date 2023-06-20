// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly;

public abstract class ResilienceStrategyBuilderBase
{
    public string? BuilderName { get; set; }
    public ResilienceProperties Properties { get; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Action<IList<ResilienceStrategy>>? OnCreatingStrategy { get; set; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DiagnosticSource? DiagnosticSource { get; set; }
}
