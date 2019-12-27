namespace Polly.Timeout
{
    /// <summary>
    /// A timeout policy that can be applied to synchronous executions.
    /// </summary>
    public interface ISyncTimeoutPolicy : ISyncPolicy, ITimeoutPolicy
    {
    }

    /// <summary>
    /// A timeout policy that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface ISyncTimeoutPolicy<TResult> : ISyncPolicy<TResult>, ITimeoutPolicy<TResult>
    {
    }
}
