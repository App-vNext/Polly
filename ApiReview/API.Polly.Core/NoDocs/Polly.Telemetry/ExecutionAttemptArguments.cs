// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Telemetry;

public class ExecutionAttemptArguments
{
    public int AttemptNumber { get; }
    public TimeSpan ExecutionTime { get; }
    public bool Handled { get; }
    public ExecutionAttemptArguments(int attemptNumber, TimeSpan executionTime, bool handled);
}
