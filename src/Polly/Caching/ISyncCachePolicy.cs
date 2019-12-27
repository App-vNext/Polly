﻿namespace Polly.Caching
{
    /// <summary>
    /// A cache policy that can be applied to synchronous executions.
    /// </summary>
    public interface ISyncCachePolicy : ISyncPolicy, ICachePolicy
    {
    }

    /// <summary>
    /// A cache policy that can be applied to synchronous executions returning a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
    public interface ISyncCachePolicy<TResult> : ISyncPolicy<TResult>, ICachePolicy<TResult>
    {
    }
}
