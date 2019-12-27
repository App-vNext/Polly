namespace Polly.Wrap
{
    /// <summary>
    /// A wrapper for composing policies that can be applied to synchronous delegate executions.
    /// </summary>
    public interface ISyncPolicyWrap : ISyncPolicy, IPolicyWrap
    {
    }

    /// <summary>
    /// A wrapper for composing policies that can be applied to synchronous delegate executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface ISyncPolicyWrap<TResult> : ISyncPolicy<TResult>, IPolicyWrap<TResult>
    {
    }
}
