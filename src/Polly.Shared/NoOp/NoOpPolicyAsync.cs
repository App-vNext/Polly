using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly.NoOp
{
    public partial class NoOpPolicy : INoOpPolicy
    {
        internal NoOpPolicy(Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy)
           : base(asyncExceptionPolicy, Enumerable.Empty<ExceptionPredicate>())
        {
        }
    }

    public partial class NoOpPolicy<TResult> : INoOpPolicy<TResult>
    {
        internal NoOpPolicy(
            Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> asyncExecutionPolicy
            ) : base(asyncExecutionPolicy, Enumerable.Empty<ExceptionPredicate>(), Enumerable.Empty<ResultPredicate<TResult>>())
        {
        }
    }
}
