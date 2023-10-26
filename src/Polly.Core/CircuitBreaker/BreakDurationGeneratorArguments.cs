using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

/// <summary>
/// Represents arguments used to generate a dynamic break duration for a circuit breaker.
/// </summary>
public class BreakDurationGeneratorArguments
{
    public BreakDurationGeneratorArguments(
        double failureRate,
        int failureCount)
    {
        FailureRate = failureRate;
        FailureCount = failureCount;
    }

    public double FailureRate { get; set; }

    public int FailureCount { get; set; }
}
