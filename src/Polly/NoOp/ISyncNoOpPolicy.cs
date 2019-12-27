namespace Polly.NoOp
{
    /// <summary>
    /// A no-op policy that can be applied to synchronous delegate executions.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    public interface ISyncNoOpPolicy : ISyncPolicy, INoOpPolicy
    {
    }

    /// <summary>
    /// A no-op policy that can be applied to synchronous delegate executions returning a value of type <typeparamref name="TResult"/>.  Code executed through the policy is executed as if no policy was applied.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface ISyncNoOpPolicy<TResult> : ISyncPolicy<TResult>, INoOpPolicy<TResult>
    {
    }
}
