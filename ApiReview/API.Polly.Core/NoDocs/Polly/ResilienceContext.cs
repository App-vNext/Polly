// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly;

public sealed class ResilienceContext
{
    public CancellationToken CancellationToken { get; set; }
    public bool IsSynchronous { get; }
    public Type ResultType { get; }
    public bool IsVoid { get; }
    public bool ContinueOnCapturedContext { get; set; }
    public ResilienceProperties Properties { get; }
    public IReadOnlyCollection<ResilienceEvent> ResilienceEvents { get; }
    public static ResilienceContext Get();
    public static void Return(ResilienceContext context);
}
