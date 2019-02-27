﻿namespace Polly.Timeout
{
    /// <summary>
    /// Defines properties and methods common to all Timeout policies.
    /// </summary>

    public interface ITimeoutPolicy : IsPolicy
    {
    }

    /// <summary>
    /// Defines properties and methods common to all Timeout policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
    /// </summary>
    public interface ITimeoutPolicy<TResult> : ITimeoutPolicy
    {

    }
}
