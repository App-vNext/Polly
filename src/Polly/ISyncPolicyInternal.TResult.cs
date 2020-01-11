using System.Threading;

namespace Polly
{
    internal interface ISyncPolicyInternal<TResult>
    {
        TResult Execute<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>;

        PolicyResult<TResult> ExecuteAndCapture<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>;
    }
}
