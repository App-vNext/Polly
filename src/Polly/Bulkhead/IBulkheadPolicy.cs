#nullable enable
namespace Polly.Bulkhead;

/// <summary>
/// Defines properties and methods common to all bulkhead policies.
/// </summary>
public interface IBulkheadPolicy : IsPolicy, IDisposable
{
    /// <summary>
    /// Gets the number of slots currently available for executing actions through the bulkhead.
    /// </summary>
    int BulkheadAvailableCount { get; }

    /// <summary>
    /// Gets the number of slots currently available for queuing actions for execution through the bulkhead.
    /// </summary>
    int QueueAvailableCount { get; }
}

/// <summary>
/// Defines properties and methods common to all bulkhead policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IBulkheadPolicy<TResult> : IBulkheadPolicy
{
}
