using System;
using System.Threading.Tasks;

namespace Polly.Utils.Pipeline;

[DebuggerDisplay("{Component}")]
internal class ExternalComponent : PipelineComponent
{
    public ExternalComponent(PipelineComponent component) => Component = component;

    internal PipelineComponent Component { get; }

    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state) => Component.ExecuteCore(callback, context, state);

    public override void Dispose()
    {
        // don't dispose component that is external
    }

    public override ValueTask DisposeAsync()
    {
        // don't dispose component that is external
        return default;
    }
}
