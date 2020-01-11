using System.Threading;
using System.Threading.Tasks;

namespace Polly
{
    internal interface IAsyncPolicyInternal
    {
        Task ExecuteAsync(IAsyncExecutable action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext);

        Task<TResult> ExecuteAsync<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>;

        Task<PolicyResult> ExecuteAndCaptureAsync(IAsyncExecutable action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext);

        Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
            where TExecutableAsync : IAsyncExecutable<TResult>;
    }
}