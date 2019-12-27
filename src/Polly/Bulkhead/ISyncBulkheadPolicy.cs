namespace Polly.Bulkhead
{
    /// <summary>
    /// A bulkhead-isolation policy which can be applied to synchronous delegate executions.
    /// </summary>
    public interface ISyncBulkheadPolicy : ISyncPolicy, IBulkheadPolicy
    {
    }

    /// <summary>
    /// A bulkhead-isolation policy which can be applied to synchronous delegate executions returning a value of type <typeparamref name="TResult"/>s.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface ISyncBulkheadPolicy<TResult> : ISyncPolicy<TResult>, IBulkheadPolicy<TResult>
    {
    }
}
