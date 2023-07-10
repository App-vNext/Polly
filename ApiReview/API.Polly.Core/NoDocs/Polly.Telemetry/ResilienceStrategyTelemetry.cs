// Assembly 'Polly.Core'

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public sealed class ResilienceStrategyTelemetry
{
    public bool IsEnabled { get; }
    public void Report<TArgs>(ResilienceEvent resilienceEvent, ResilienceContext context, TArgs args);
    public void Report<TArgs, TResult>(ResilienceEvent resilienceEvent, OutcomeArguments<TResult, TArgs> args);
}
