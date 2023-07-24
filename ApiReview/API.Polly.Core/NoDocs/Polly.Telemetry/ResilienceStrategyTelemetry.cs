// Assembly 'Polly.Core'

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public sealed class ResilienceStrategyTelemetry
{
    public bool IsEnabled { get; }
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Reflection is not used when consuming the event.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Reflection is not used when consuming the event.")]
    public void Report<TArgs>(ResilienceEvent resilienceEvent, ResilienceContext context, TArgs args);
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Reflection is not used when consuming the event.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Reflection is not used when consuming the event.")]
    public void Report<TArgs, TResult>(ResilienceEvent resilienceEvent, OutcomeArguments<TResult, TArgs> args);
}
