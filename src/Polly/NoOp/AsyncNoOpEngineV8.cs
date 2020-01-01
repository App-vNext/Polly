using System.Threading;
using System.Threading.Tasks;

namespace Polly.NoOp
{
    internal static class AsyncNoOpEngineV8
    {
        internal static Task<TResult> ExecuteAsync<TExecutableAsync, TResult>(TExecutableAsync action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) 
            where TExecutableAsync : IAsyncExecutable<TResult>
        {
            return action.ExecuteAsync(context, cancellationToken, continueOnCapturedContext);
        }
    }
}
