// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Telemetry;

public sealed class ExecutionAttemptArguments
{
    public int AttemptNumber { get; }
    public TimeSpan Duration { get; }
    public bool Handled { get; }
    public ExecutionAttemptArguments(int attemptNumber, TimeSpan duration, bool handled);
}
