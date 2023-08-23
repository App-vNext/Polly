// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public readonly struct ExecutionAttemptArguments
{
    public int AttemptNumber { get; }
    public TimeSpan Duration { get; }
    public bool Handled { get; }
    public ExecutionAttemptArguments(int attemptNumber, TimeSpan duration, bool handled);
}
