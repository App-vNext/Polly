using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Utilities;

namespace Polly
{
    /// <inheritdoc/>
    internal readonly struct AsyncExecutableActionNoParams : IAsyncExecutable
    {
        private readonly Func<Task> _action;

        public AsyncExecutableActionNoParams(Func<Task> action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public readonly async Task<object> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            await _action().ConfigureAwait(continueOnCapturedContext);
           return null;
        }
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableActionOnContext : IAsyncExecutable
    {
        private readonly Func<Context, Task> _action;

        public AsyncExecutableActionOnContext(Func<Context, Task> action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public readonly async Task<object> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            await _action(context).ConfigureAwait(continueOnCapturedContext);
           return null;
        }
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableActionOnCancellationToken : IAsyncExecutable
    {
        private readonly Func<CancellationToken, Task> _action;

        public AsyncExecutableActionOnCancellationToken(Func<CancellationToken, Task> action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public readonly async Task<object> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            await _action(cancellationToken).ConfigureAwait(continueOnCapturedContext);
           return null;
        }
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableActionOnContextCancellationToken : IAsyncExecutable
    {
        private readonly Func<Context, CancellationToken, Task> _action;

        public AsyncExecutableActionOnContextCancellationToken(Func<Context, CancellationToken, Task> action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public readonly async Task<object> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            await _action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
           return null;
        }
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableAction : IAsyncExecutable
    {
        private readonly Func<Context, CancellationToken, bool, Task> _action;

        public AsyncExecutableAction(Func<Context, CancellationToken, bool, Task> action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public readonly async Task<object> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            await _action(context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
           return null;
        }
    }

    internal readonly struct AsyncExecutableAction<T1> : IAsyncExecutable
    {
        private readonly Func<Context, CancellationToken, bool, T1, Task> _action;
        private readonly T1 _arg1;

        public AsyncExecutableAction(Func<Context, CancellationToken, bool, T1, Task> action, T1 arg1)
        {
            _action = action;
            _arg1 = arg1;
        }

        public readonly async Task<object> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            await _action(context, cancellationToken, continueOnCapturedContext, _arg1).ConfigureAwait(continueOnCapturedContext);
           return null;
        }
    }

    internal readonly struct AsyncExecutableAction<T1, T2> : IAsyncExecutable
    {
        private readonly Func<Context, CancellationToken, bool, T1, T2, Task> _action;
        private readonly T1 _arg1;
        private readonly T2 _arg2;

        public AsyncExecutableAction(Func<Context, CancellationToken, bool, T1, T2, Task> action, T1 arg1, T2 arg2)
        {
            _action = action;
            _arg1 = arg1;
            _arg2 = arg2;
        }

        public readonly async Task<object> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            await _action(context, cancellationToken, continueOnCapturedContext, _arg1, _arg2).ConfigureAwait(continueOnCapturedContext);
           return null;
        }
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableFuncNoParams<TResult> : IAsyncExecutable<TResult>
    {
        private readonly Func<Task<TResult>> _func;

        /// <summary>
        /// Creates a <see cref="AsyncExecutableFuncNoParams{TResult}"/> struct for the passed func, which may be executed through a policy at a later point in time.
        /// </summary>
        /// <param name="func">The function.</param>
        public AsyncExecutableFuncNoParams(Func<Task<TResult>> func)
        {
            _func = func;
        }

        /// <inheritdoc/>
        public readonly Task<TResult> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) => _func();
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableFuncOnContext<TResult> : IAsyncExecutable<TResult>
    {
        private readonly Func<Context, Task<TResult>> _func;

        /// <summary>
        /// Creates a <see cref="AsyncExecutableFuncOnContext{TResult}"/> struct for the passed func, which may be executed through a policy at a later point in time.
        /// </summary>
        /// <param name="func">The function.</param>
        public AsyncExecutableFuncOnContext(Func<Context, Task<TResult>> func)
        {
            _func = func;
        }

        /// <inheritdoc/>
        public readonly Task<TResult> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) => _func(context);
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableFuncOnCancellationToken<TResult> : IAsyncExecutable<TResult>
    {
        private readonly Func<CancellationToken, Task<TResult>> _func;

        /// <summary>
        /// Creates a <see cref="AsyncExecutableFuncOnCancellationToken{TResult}"/> struct for the passed func, which may be executed through a policy at a later point in time.
        /// </summary>
        /// <param name="func">The function.</param>
        public AsyncExecutableFuncOnCancellationToken(Func<CancellationToken, Task<TResult>> func)
        {
            _func = func;
        }

        /// <inheritdoc/>
        public readonly Task<TResult> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) => _func(cancellationToken);
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableFuncOnContextCancellationToken<TResult> : IAsyncExecutable<TResult>
    {
        private readonly Func<Context, CancellationToken, Task<TResult>> _func;

        /// <summary>
        /// Creates a <see cref="AsyncExecutableFuncOnContextCancellationToken{TResult}"/> struct for the passed func, which may be executed through a policy at a later point in time.
        /// </summary>
        /// <param name="func">The function.</param>
        public AsyncExecutableFuncOnContextCancellationToken(Func<Context, CancellationToken, Task<TResult>> func)
        {
            _func = func;
        }

        /// <inheritdoc/>
        public readonly Task<TResult> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) => _func(context, cancellationToken);
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableFunc<TResult> : IAsyncExecutable<TResult>
    {
        private readonly Func<Context, CancellationToken, bool, Task<TResult>> _func;

        /// <summary>
        /// Creates a <see cref="AsyncExecutableFunc{TResult}"/> struct for the passed func, which may be executed through a policy at a later point in time.
        /// </summary>
        /// <param name="func">The function.</param>
        public AsyncExecutableFunc(Func<Context, CancellationToken, bool, Task<TResult>> func)
        {
            _func = func;
        }

        /// <inheritdoc/>
        public readonly Task<TResult> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) => _func(context, cancellationToken, continueOnCapturedContext);
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableFunc<T1, TResult> : IAsyncExecutable<TResult>
    {
        private readonly Func<Context, CancellationToken, bool, T1, Task<TResult>> _func;
        private readonly T1 _arg1;

        public AsyncExecutableFunc(Func<Context, CancellationToken, bool, T1, Task<TResult>> func, T1 arg1)
        {
            _func = func;
            _arg1 = arg1;
        }

        /// <inheritdoc/>
        public readonly Task<TResult> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) => _func(context, cancellationToken, continueOnCapturedContext, _arg1);
    }

    /// <inheritdoc/>
    internal readonly struct AsyncExecutableFunc<T1, T2, TResult> : IAsyncExecutable<TResult>
    {
        private readonly Func<Context, CancellationToken, bool, T1, T2, Task<TResult>> _func;
        private readonly T1 _arg1;
        private readonly T2 _arg2;

        public AsyncExecutableFunc(Func<Context, CancellationToken, bool, T1, T2, Task<TResult>> func, T1 arg1, T2 arg2)
        {
            _func = func;
            _arg1 = arg1;
            _arg2 = arg2;
        }

        /// <inheritdoc/>
        public readonly Task<TResult> ExecuteAsync(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) => _func(context, cancellationToken, continueOnCapturedContext, _arg1, _arg2);
    }
}
