using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    internal interface IAsyncPolicyInternal
    {
        Task ExecuteAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable;

        Task<TResult> ExecuteAsync<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>;

        Task<PolicyResult> ExecuteAndCaptureAsync<TExecutableAsync>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable;


        Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>;
    }
}