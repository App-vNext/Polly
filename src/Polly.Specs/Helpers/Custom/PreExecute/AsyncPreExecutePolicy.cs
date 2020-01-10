using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers.Custom.PreExecute
{
    internal class AsyncPreExecutePolicy : AsyncPolicyV8
    {
        private Func<Task> _preExecute;

        public static AsyncPreExecutePolicy CreateAsync(Func<Task> preExecute)
        {
            return new AsyncPreExecutePolicy(preExecute);
        }

        internal AsyncPreExecutePolicy(Func<Task> preExecute)
        {
            _preExecute = preExecute ?? throw new ArgumentNullException(nameof(preExecute));
        }

        protected override Task<TResult> ImplementationAsyncV8<TExecutableAsync, TResult>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncPreExecuteEngine.ImplementationAsync<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext, _preExecute);
        }
    }

    internal class AsyncPreExecutePolicy<TResult> : AsyncPolicyV8<TResult>
    {
        private Func<Task> _preExecute;

        public static AsyncPreExecutePolicy<TResult> CreateAsync(Func<Task> preExecute)
        {
            return new AsyncPreExecutePolicy<TResult>(preExecute);
        }

        internal AsyncPreExecutePolicy(Func<Task> preExecute)
        {
            _preExecute = preExecute ?? throw new ArgumentNullException(nameof(preExecute));
        }

        protected override Task<TResult> ImplementationAsyncV8<TExecutableAsync>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncPreExecuteEngine.ImplementationAsync<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext, _preExecute);
        }
    }
}