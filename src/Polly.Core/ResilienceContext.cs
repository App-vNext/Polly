using System.Diagnostics.CodeAnalysis;
using Polly.Telemetry;

namespace Polly;

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters

/// <summary>
/// A context assigned to a single execution of <see cref="ResilienceStrategy"/>. It is created manually or automatically
/// when the user calls the various extensions on top of <see cref="ResilienceStrategy"/>. After every execution the context should be discarded and returned to the pool.
/// </summary>
/// <remarks>
/// Do not re-use an instance of <see cref="ResilienceContext"/> across more than one execution. The <see cref="ResilienceContext"/> is retrieved from the pool
/// by calling the <see cref="Get(CancellationToken)"/> method. After you are done with it you should return it to the pool by calling the <see cref="Return"/> method.
/// </remarks>
public sealed class ResilienceContext
{
    private const bool ContinueOnCapturedContextDefault = false;

    private static readonly ObjectPool<ResilienceContext> Pool = new(static () => new ResilienceContext(), static c => c.Reset());

    private readonly List<ResilienceEvent> _resilienceEvents = new();

    private ResilienceContext()
    {
    }

    /// <summary>
    /// Gets a key unique to the call site of the current execution.
    /// </summary>
    /// <remarks>
    /// Resilience strategy instances are commonly reused across multiple call sites.
    /// Set an <see cref="OperationKey"/> so that logging and metrics can distinguish usages of policy instances at different call sites.
    /// The operation key value should have a low cardinality (i.e. do not assign values such as <see cref="Guid"/> to this property).
    /// </remarks>
    /// <value>The default value is <see langword="null"/>.</value>
    public string? OperationKey { get; private set; }

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with the execution.
    /// </summary>
    public CancellationToken CancellationToken { get; internal set; }

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
    public IReadOnlyList<ResilienceEvent> ResilienceEvents => _resilienceEvents;

    /// <summary>
    /// Gets a <see cref="ResilienceContext"/> instance from the pool.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of <see cref="ResilienceContext"/>.</returns>
    /// <remarks>
    /// After the execution is finished you should return the <see cref="ResilienceContext"/> back to the pool
    /// by calling <see cref="Return(ResilienceContext)"/> method.
    /// </remarks>
    public static ResilienceContext Get(CancellationToken cancellationToken = default)
    {
        var context = Pool.Get();
        context.CancellationToken = cancellationToken;
        return context;
    }

    /// <summary>
    /// Gets a <see cref="ResilienceContext"/> instance from the pool.
    /// </summary>
    /// <param name="operationKey">An operation key associated with the context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of <see cref="ResilienceContext"/>.</returns>
    /// <remarks>
    /// After the execution is finished you should return the <see cref="ResilienceContext"/> back to the pool
    /// by calling <see cref="Return(ResilienceContext)"/> method.
    /// </remarks>
    public static ResilienceContext Get(string operationKey, CancellationToken cancellationToken = default)
    {
        var context = Pool.Get();
        context.OperationKey = operationKey;
        context.CancellationToken = cancellationToken;
        return context;
    }

    internal void InitializeFrom(ResilienceContext context)
    {
        OperationKey = context.OperationKey;
        ResultType = context.ResultType;
        IsSynchronous = context.IsSynchronous;
        CancellationToken = context.CancellationToken;
        ContinueOnCapturedContext = context.ContinueOnCapturedContext;
        _resilienceEvents.Clear();
        _resilienceEvents.AddRange(context.ResilienceEvents);
    }

    /// <summary>
    /// Returns a <paramref name="context"/> back to the pool.
    /// </summary>
    /// <param name="context">The context instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    public static void Return(ResilienceContext context)
    {
        Guard.NotNull(context);

        Pool.Return(context);
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

    internal void AddResilienceEvent(ResilienceEvent @event)
    {
        _resilienceEvents.Add(@event);
    }

    internal bool Reset()
    {
        OperationKey = null;
        IsSynchronous = false;
        ResultType = typeof(UnknownResult);
        ContinueOnCapturedContext = false;
        CancellationToken = default;
        Properties.Options.Clear();
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
