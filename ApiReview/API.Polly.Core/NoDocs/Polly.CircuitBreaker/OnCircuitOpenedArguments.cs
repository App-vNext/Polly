// Assembly 'Polly.Core'

using System;

namespace Polly.CircuitBreaker;

public readonly record struct OnCircuitOpenedArguments(TimeSpan BreakDuration, bool IsManual);
