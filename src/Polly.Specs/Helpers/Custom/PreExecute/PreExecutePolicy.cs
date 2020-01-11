using System;
using System.Threading;

namespace Polly.Specs.Helpers.Custom.PreExecute
{
    internal class PreExecutePolicy : PolicyV8
    {
        private Action _preExecute;

        public static PreExecutePolicy Create(Action preExecute)
        {
            return new PreExecutePolicy(preExecute);
        }

        internal PreExecutePolicy(Action preExecute)
        {
            _preExecute = preExecute ?? throw new ArgumentNullException(nameof(preExecute));
        }

        protected override TResult SyncGenericImplementationV8<TExecutable, TResult>(in TExecutable action, Context context,
            CancellationToken cancellationToken)
        {
            return PreExecuteEngine.Implementation<TExecutable, TResult>(action, context, cancellationToken, _preExecute);
        }
    }

    internal class PreExecutePolicy<TResult> : PolicyV8<TResult>
    {
        private Action _preExecute;

        public static PreExecutePolicy<TResult> Create(Action preExecute)
        {
            return new PreExecutePolicy<TResult>(preExecute);
        }

        internal PreExecutePolicy(Action preExecute)
        {
            _preExecute = preExecute ?? throw new ArgumentNullException(nameof(preExecute));
        }

        protected override TResult SyncGenericImplementationV8<TExecutable>(in TExecutable action, Context context, CancellationToken cancellationToken)
        {
            return PreExecuteEngine.Implementation<TExecutable, TResult>(action, context, cancellationToken, _preExecute);
        }
    }
}