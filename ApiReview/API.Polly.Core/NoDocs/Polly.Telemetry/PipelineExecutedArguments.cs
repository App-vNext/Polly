// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public readonly struct PipelineExecutedArguments
{
    public TimeSpan Duration { get; }
    public PipelineExecutedArguments(TimeSpan duration);
}
