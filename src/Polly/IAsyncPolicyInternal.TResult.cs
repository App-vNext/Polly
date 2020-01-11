using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    internal interface IAsyncPolicyInternal<TResult>
    {
        Task<TResult> ExecuteAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>;

        Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>;
    }
}