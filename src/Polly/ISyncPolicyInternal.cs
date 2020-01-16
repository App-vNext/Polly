using System.Threading;

namespace Polly
{
    internal interface ISyncPolicyInternal
    {
        void Execute<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable;

        TResult Execute<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>;

        PolicyResult ExecuteAndCapture<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable;

        PolicyResult<TResult> ExecuteAndCapture<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>;
    }
}
