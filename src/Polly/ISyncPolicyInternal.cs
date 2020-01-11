using System.Threading;

namespace Polly
{
    internal interface ISyncPolicyInternal
    {
        void Execute(in ISyncExecutable action, Context context, CancellationToken cancellationToken);

        TResult Execute<TExecutable, TResult>(in TExecutable action, Context context,
            CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>;

        PolicyResult ExecuteAndCapture(in ISyncExecutable action, Context context, CancellationToken cancellationToken);

        PolicyResult<TResult> ExecuteAndCapture<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>;
    }
}
