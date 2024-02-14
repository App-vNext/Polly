using System.Diagnostics.CodeAnalysis;

namespace Polly;

/// <summary>
/// A context assigned to a single execution of <see cref="ResiliencePipeline"/>. It is created manually or automatically
/// when the user calls the various extensions on top of <see cref="ResiliencePipeline"/>. After every execution the context should be discarded and returned to the pool.
/// </summary>
/// <remarks>
/// Do not re-use an instance of <see cref="ResilienceContext"/> across more than one execution. The <see cref="ResilienceContext"/> is retrieved from the pool
/// by calling the <see cref="ResilienceContextPool.Get(CancellationToken)"/> method. After you are done with it you should return it to the pool
/// by calling the <see cref="ResilienceContextPool.Return(ResilienceContext)"/> method.
/// </remarks>
public sealed class ResilienceContext
{
    internal ResilienceContext()
    {
    }

    /// <summary>
    /// Gets a key unique to the call site of the current execution.
    /// </summary>
    /// <remarks>
    /// Resilience context instances are commonly reused across multiple call sites.
    /// Set an <see cref="OperationKey"/> so that logging and metrics can distinguish usages of policy instances at different call sites.
    /// The operation key value should have a low cardinality (i.e. do not assign values such as <see cref="Guid"/> to this property).
    /// </remarks>
    /// <value>The default value is <see langword="null"/>.</value>
    public string? OperationKey { get; internal set; }

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with the execution.
    /// </summary>
    public CancellationToken CancellationToken { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the execution is synchronous.
    /// </summary>
    internal bool IsSynchronous { get; private set; }

    /// <summary>
    /// Gets the type of the result associated with the execution.
    /// </summary>
    internal Type ResultType { get; private set; } = typeof(UnknownResult);

    /// <summary>
    /// Gets a value indicating whether the execution represents a void result.
    /// </summary>
    internal bool IsVoid => ResultType == typeof(VoidResult);

    /// <summary>
    /// Gets a value indicating whether the execution should continue on the captured context.
    /// </summary>
    public bool ContinueOnCapturedContext { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the context is initialized.
    /// </summary>
    internal bool IsInitialized => ResultType != typeof(UnknownResult);

    /// <summary>
    /// Gets the custom properties attached to the context.
    /// </summary>
    public ResilienceProperties Properties { get; } = new();

    internal void InitializeFrom(ResilienceContext context, CancellationToken cancellationToken)
    {
        OperationKey = context.OperationKey;
        ResultType = context.ResultType;
        IsSynchronous = context.IsSynchronous;
        CancellationToken = context.CancellationToken;
        ContinueOnCapturedContext = context.ContinueOnCapturedContext;
        CancellationToken = cancellationToken;
        Properties.AddOrReplaceProperties(context.Properties);
    }

    [ExcludeFromCodeCoverage]
    [Conditional("DEBUG")]
    internal void AssertInitialized() => Debug.Assert(IsInitialized, "The resilience context is not initialized.");

    internal ResilienceContext Initialize<TResult>(bool isSynchronous)
    {
        IsSynchronous = isSynchronous;
        ResultType = typeof(TResult);

        return this;
    }

    internal bool Reset()
    {
        OperationKey = null;
        IsSynchronous = false;
        ResultType = typeof(UnknownResult);
        ContinueOnCapturedContext = false;
        CancellationToken = default;
        Properties.Options.Clear();
        return true;
    }

    /// <summary>
    /// Marker class that represents an unknown result.
    /// </summary>
    private static class UnknownResult
    {
    }
}
