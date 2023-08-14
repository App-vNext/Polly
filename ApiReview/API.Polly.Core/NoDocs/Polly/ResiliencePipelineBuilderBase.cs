// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Polly;

public abstract class ResiliencePipelineBuilderBase
{
    public string? Name { get; set; }
    public string? InstanceName { get; set; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DiagnosticSource? DiagnosticSource { get; set; }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Action<ResilienceValidationContext> Validator { get; }
}
