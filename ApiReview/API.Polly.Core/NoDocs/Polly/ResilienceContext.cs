// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Polly.Telemetry;

namespace Polly;

public sealed class ResilienceContext
{
    public string? OperationKey { get; }
    public CancellationToken CancellationToken { get; }
    public bool ContinueOnCapturedContext { get; }
    public ResilienceProperties Properties { get; }
    public IReadOnlyList<ResilienceEvent> ResilienceEvents { get; }
}
