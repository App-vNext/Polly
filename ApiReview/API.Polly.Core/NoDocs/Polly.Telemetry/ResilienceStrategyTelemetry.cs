// Assembly 'Polly.Core'

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public sealed class ResilienceStrategyTelemetry
{
    public void Report<TArgs>(string eventName, ResilienceContext context, TArgs args);
    public void Report<TArgs, TResult>(string eventName, OutcomeArguments<TResult, TArgs> args);
}
