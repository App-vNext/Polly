// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public sealed class ResilienceStrategyTelemetry
{
    public void Report<TArgs>(ResilienceEvent resilienceEvent, ResilienceContext context, TArgs args);
    public void Report<TArgs, TResult>(ResilienceEvent resilienceEvent, OutcomeArguments<TResult, TArgs> args);
}
