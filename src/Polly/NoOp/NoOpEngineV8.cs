﻿using System.Threading;

namespace Polly.NoOp
{
    internal static class NoOpEngine
    {
        internal static TResult Implementation<TExecutable, TResult>(in TExecutable action, Context context, CancellationToken cancellationToken)
            where TExecutable : ISyncExecutable<TResult>
        {
            return action.Execute(context, cancellationToken);
        }
    }
}
