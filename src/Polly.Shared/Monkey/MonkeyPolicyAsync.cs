using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Monkey
{
    public partial class MonkeyPolicy : IMonkeyPolicy
    {
        internal MonkeyPolicy(Func<Func<Context, CancellationToken, Task>, Context, CancellationToken, bool, Task> asyncExceptionPolicy)
           : base(asyncExceptionPolicy, Enumerable.Empty<ExceptionPredicate>())
        {
        }
    }

    public partial class MonkeyPolicy<TResult> : IMonkeyPolicy<TResult>
    {
        internal MonkeyPolicy(
            Func<Func<Context, CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> asyncExecutionPolicy
            ) : base(asyncExecutionPolicy, Enumerable.Empty<ExceptionPredicate>(), Enumerable.Empty<ResultPredicate<TResult>>())
        {
        }
    }
}
