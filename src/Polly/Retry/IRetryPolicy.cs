using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Retry
{
    /// <summary>
    /// Defines properties and methods common to all Retry policies.
    /// </summary>

    public interface IRetryPolicy : IsPolicy
    {
    }

    /// <summary>
    /// Defines properties and methods common to all Retry policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
    /// </summary>
    public interface IRetryPolicy<TResult> : IRetryPolicy
    {

    }
}
