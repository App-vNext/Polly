using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.Shared.NoOp
{
    /// <summary>
    /// An async no op policy that can be applied to delegates.
    /// </summary>
    public partial class NoOpPolicy
    {
        internal NoOpPolicy(Func<Func<CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy)
           : base(asyncExceptionPolicy, Enumerable.Empty<ExceptionPredicate>())
        {
        }
    }

    /// <summary>
    /// An async no op policy that can be applied to delegates returning a value of type <typeparamref name="TResult" />.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public partial class NoOpPolicy<TResult>
    {
        internal NoOpPolicy(
            Func<Func<CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> asyncExecutionPolicy
            ) : base(asyncExecutionPolicy, Enumerable.Empty<ExceptionPredicate>(), Enumerable.Empty<ResultPredicate<TResult>>())
        {
        }
    }
}
