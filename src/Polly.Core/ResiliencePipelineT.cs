namespace Polly;

/// <summary>
/// Resilience pipeline is used to execute the user-provided callbacks.
/// </summary>
/// <typeparam name="T">The type of result this pipeline supports.</typeparam>
/// <remarks>
/// Resilience ppeline supports various types of callbacks of <typeparamref name="T"/> result type
/// and provides a unified way to execute them. This includes overloads for synchronous and asynchronous callbacks.
/// </remarks>
public partial class ResiliencePipeline<T>
{
    internal ResiliencePipeline(ResiliencePipeline strategy) => Strategy = strategy;

    internal ResiliencePipeline Strategy { get; }
}
