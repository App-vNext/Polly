using System.Threading;

namespace Polly.NoOp
{
    internal static class NoOpEngineV8
    {
        internal static TResult Execute<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>
        {
            return action.Execute(context, cancellationToken);
        }
    }
}
