using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Wrap
{
    public partial class PolicyWrap
    {
        internal PolicyWrap(Func<Func<CancellationToken, Task>, Context, CancellationToken, bool, Task> policyAction)
            : base(policyAction, Enumerable.Empty<ExceptionPredicate>())
        {
        }
    }

    public partial class PolicyWrap<TResult>
    {
        internal PolicyWrap(Func<Func<CancellationToken, Task<TResult>>, Context, CancellationToken, bool, Task<TResult>> policyAction)
            : base(policyAction, Enumerable.Empty<ExceptionPredicate>(), Enumerable.Empty<ResultPredicate<TResult>>())
        {
        }
    }
}
