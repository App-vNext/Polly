namespace Polly.Bulkhead
{
    /// <summary>
    /// A bulkhead-isolation policy which can be applied to delegates.
    /// </summary>
    public interface ISyncBulkheadPolicy : ISyncPolicy, IBulkheadPolicy
    {
    }

    /// <summary>
    /// A bulkhead-isolation policy which can be applied to delegates.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface ISyncBulkheadPolicy<TResult> : ISyncPolicy<TResult>, IBulkheadPolicy<TResult>
    {
    }
}
