using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.Specs.Helpers.Custom.PreExecute
{
    internal class AsyncPreExecutePolicy : AsyncPolicy
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

        protected override Task<TResult> AsyncGenericImplementation<TExecutableAsync, TResult>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncPreExecuteEngine.ImplementationAsync<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext, _preExecute);
        }
    }

    internal class AsyncPreExecutePolicy<TResult> : AsyncPolicy<TResult>
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

        protected override Task<TResult> AsyncGenericImplementation<TExecutableAsync>(TExecutableAsync action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            return AsyncPreExecuteEngine.ImplementationAsync<TExecutableAsync, TResult>(action, context, cancellationToken, continueOnCapturedContext, _preExecute);
        }
    }
}