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

        protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncPreExecuteEngine.ImplementationAsync(_preExecute, action, context, cancellationToken, continueOnCapturedContext);
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

        protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            return AsyncPreExecuteEngine.ImplementationAsync(_preExecute, action, context, cancellationToken, continueOnCapturedContext);
        }
    }
}