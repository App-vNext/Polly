using Polly.Utils.Pipeline;

namespace Polly;

/// <summary>
/// Resilience pipeline is used to execute the user-provided callbacks.
/// </summary>
/// <typeparam name="T">The type of result this pipeline supports.</typeparam>
/// <remarks>
/// Resilience pipeline supports various types of callbacks of <typeparamref name="T"/> result type
/// and provides a unified way to execute them. This includes overloads for synchronous and asynchronous callbacks.
/// </remarks>
public sealed partial class ResiliencePipeline<T>
{
    /// <summary>
    /// Resilience pipeline that executes the user-provided callback without any additional logic.
    /// </summary>
    public static readonly ResiliencePipeline<T> Empty = new(PipelineComponent.Empty, DisposeBehavior.Ignore);

    internal ResiliencePipeline(PipelineComponent component, DisposeBehavior disposeBehavior)
    {
        // instead of re-implementing individual Execute* methods we
        // can just re-use the non-generic ResiliencePipeline and
        // call it from Execute* methods in this class
        Pipeline = new ResiliencePipeline(component, disposeBehavior);
        DisposeHelper = Pipeline.DisposeHelper;
    }

    internal PipelineComponent Component => Pipeline.Component;

    internal ComponentDisposeHelper DisposeHelper { get; }

    private ResiliencePipeline Pipeline { get; }
}
