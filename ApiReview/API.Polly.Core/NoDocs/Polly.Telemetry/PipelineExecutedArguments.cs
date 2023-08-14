// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Telemetry;

public sealed class PipelineExecutedArguments
{
    public TimeSpan Duration { get; }
    public PipelineExecutedArguments(TimeSpan duration);
}
