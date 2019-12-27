namespace Polly.Bulkhead
{
    /// <summary>
    /// A bulkhead-isolation policy which can be applied to asynchronous delegate executions.
    /// </summary>
    public interface IAsyncBulkheadPolicy : IAsyncPolicy, IBulkheadPolicy
    {
    }

    /// <summary>
    /// A bulkhead-isolation policy which can be applied to asynchronous delegate executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface IAsyncBulkheadPolicy<TResult> : IAsyncPolicy<TResult>, IBulkheadPolicy<TResult>
    {
    }
}
