using System.Diagnostics.CodeAnalysis;
using Polly.Strategy;

namespace Polly;

/// <summary>
/// A context assigned to a single execution of <see cref="ResilienceStrategy"/>. It is created manually or automatically
/// when the user calls the various extensions on top of <see cref="ResilienceStrategy"/>. After every execution the context should be discarded and returned to the pool.
/// </summary>
/// <remarks>
/// Do not re-use an instance of <see cref="ResilienceContext"/> across more than one execution. The <see cref="ResilienceContext"/> is retrieved from the pool
/// by calling the <see cref="ResilienceContextPool.Get"/> method. After you are done with it,
/// you should return it to the pool by calling the <see cref="ResilienceContextPool.Return(ResilienceContext)"/> method.
/// </remarks>
public sealed class ResilienceContext
{
    private const bool ContinueOnCapturedContextDefault = false;

    private readonly List<ReportedResilienceEvent> _resilienceEvents = new();

    internal ResilienceContext()
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="CancellationToken"/> associated with the execution.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Gets a value indicating whether the execution is synchronous.
    /// </summary>
    public bool IsSynchronous { get; private set; }

    /// <summary>
    /// Gets the type of the result associated with the execution.
    /// </summary>
    public Type ResultType { get; private set; } = typeof(UnknownResult);

    /// <summary>
    /// Gets a value indicating whether the execution represents a void result.
    /// </summary>
    public bool IsVoid => ResultType == typeof(VoidResult);

    /// <summary>
    /// Gets or sets a value indicating whether the execution should continue on the captured context.
    /// </summary>
    public bool ContinueOnCapturedContext { get; set; }

    /// <summary>
    /// Gets a value indicating whether the context is initialized.
    /// </summary>
    internal bool IsInitialized => ResultType != typeof(UnknownResult);

    /// <summary>
    /// Gets the custom properties attached to the context.
    /// </summary>
    public ResilienceProperties Properties { get; internal set; } = new();

    /// <summary>
    /// Gets the collection of resilience events that occurred while executing the resilience strategy.
    /// </summary>
    /// <remarks>
    /// If the number of resilience events is greater than zero it's an indication that the execution was unhealthy.
    /// </remarks>
    public IReadOnlyCollection<ReportedResilienceEvent> ResilienceEvents => _resilienceEvents;

    internal void InitializeFrom(ResilienceContext context)
    {
        ResultType = context.ResultType;
        IsSynchronous = context.IsSynchronous;
        CancellationToken = context.CancellationToken;
        ContinueOnCapturedContext = context.ContinueOnCapturedContext;
        _resilienceEvents.Clear();
        _resilienceEvents.AddRange(context.ResilienceEvents);
    }

    [ExcludeFromCodeCoverage]
    [Conditional("DEBUG")]
    internal void AssertInitialized()
    {
        Debug.Assert(IsInitialized, "The resilience context is not initialized.");
    }

    internal ResilienceContext Initialize<TResult>(bool isSynchronous)
    {
        IsSynchronous = isSynchronous;
        ResultType = typeof(TResult);
        ContinueOnCapturedContext = ContinueOnCapturedContextDefault;

        return this;
    }

    internal void AddResilienceEvent(ReportedResilienceEvent @event)
    {
        _resilienceEvents.Add(@event);
    }

    internal bool Reset()
    {
        IsSynchronous = false;
        ResultType = typeof(UnknownResult);
        ContinueOnCapturedContext = false;
        CancellationToken = default;
        ((IDictionary<string, object?>)Properties).Clear();
        _resilienceEvents.Clear();
        return true;
    }

    /// <summary>
    /// Marker class that represents an unknown result.
    /// </summary>
    private static class UnknownResult
    {
    }
}
