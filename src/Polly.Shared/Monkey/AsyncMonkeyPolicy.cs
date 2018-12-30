using System;
using System.Threading.Tasks;

namespace Polly.Monkey
{
    /// <summary>
    /// Contains common functionality for policies which intentionally disrupt async executions - which monkey around with calls.
    /// </summary>
    public abstract class AsyncMonkeyPolicy : AsyncPolicy, IMonkeyPolicy
    {
        internal Func<Context, Task<Double>> InjectionRate { get; }
        internal Func<Context, Task<bool>> Enabled { get; }

        internal AsyncMonkeyPolicy(Func<Context, Task<Double>> injectionRate, Func<Context, Task<bool>> enabled)
           : base(ExceptionPredicates.None)
        {
            InjectionRate = injectionRate ?? throw new ArgumentNullException(nameof(injectionRate));
            Enabled = enabled ?? throw new ArgumentNullException(nameof(enabled));
        }
    }

    /// <summary>
    /// Contains common functionality for policies which intentionally disrupt async executions returning TResult - which monkey around with calls.
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    public abstract class AsyncMonkeyPolicy<TResult> : AsyncPolicy<TResult>, IMonkeyPolicy<TResult>
    {
        internal Func<Context, Task<Double>> InjectionRate { get; }
        internal Func<Context, Task<bool>> Enabled { get; }

        internal AsyncMonkeyPolicy(Func<Context, Task<Double>> injectionRate, Func<Context, Task<bool>> enabled) 
            : base(ExceptionPredicates.None, ResultPredicates<TResult>.None)
        {
            InjectionRate = injectionRate ?? throw new ArgumentNullException(nameof(injectionRate));
            Enabled = enabled ?? throw new ArgumentNullException(nameof(enabled));
        }
    }
}
