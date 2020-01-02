using System;
using System.Threading;
using Polly.Utilities;

namespace Polly
{
    /// <inheritdoc/>
    internal readonly struct SyncExecutableActionNoParams : ISyncExecutable<EmptyStruct>
    {
        private readonly Action _action;

        public SyncExecutableActionNoParams(Action action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public readonly EmptyStruct Execute(Context context, CancellationToken cancellationToken)
        {
            _action();
            return EmptyStruct.Instance;
        }
    }

    /// <inheritdoc/>
    internal readonly struct SyncExecutableActionOnContext : ISyncExecutable<EmptyStruct>
    {
        private readonly Action<Context> _action;

        public SyncExecutableActionOnContext(Action<Context> action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public readonly EmptyStruct Execute(Context context, CancellationToken cancellationToken)
        {
            _action(context);
            return EmptyStruct.Instance;
        }
    }

    /// <inheritdoc/>
    internal readonly struct SyncExecutableActionOnCancellationToken : ISyncExecutable<EmptyStruct>
    {
        private readonly Action<CancellationToken> _action;

        public SyncExecutableActionOnCancellationToken(Action<CancellationToken> action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public readonly EmptyStruct Execute(Context context, CancellationToken cancellationToken)
        {
            _action(cancellationToken);
            return EmptyStruct.Instance;
        }
    }

    /// <inheritdoc/>
    internal readonly struct SyncExecutableAction : ISyncExecutable<EmptyStruct>
    {
        private readonly Action<Context, CancellationToken> _action;

        public SyncExecutableAction(Action<Context, CancellationToken> action)
        {
            _action = action;
        }

        /// <inheritdoc/>
        public readonly EmptyStruct Execute(Context context, CancellationToken cancellationToken)
        {
            _action(context, cancellationToken);
            return EmptyStruct.Instance;
        }
    }
    
    internal readonly struct SyncExecutableAction<T1> : ISyncExecutable<EmptyStruct>
    {
        private readonly Action<Context, CancellationToken, T1> _action;
        private readonly T1 _arg1;

        public SyncExecutableAction(Action<Context, CancellationToken, T1> action, T1 arg1)
        {
            _action = action;
            _arg1 = arg1;
        }

        public readonly EmptyStruct Execute(Context context, CancellationToken cancellationToken)
        {
            _action(context, cancellationToken, _arg1);
            return EmptyStruct.Instance;
        }
    }

    internal readonly struct SyncExecutableAction<T1, T2> : ISyncExecutable<EmptyStruct>
    {
        private readonly Action<Context, CancellationToken, T1, T2> _action;
        private readonly T1 _arg1;
        private readonly T2 _arg2;

        public SyncExecutableAction(Action<Context, CancellationToken, T1, T2> action, T1 arg1, T2 arg2)
        {
            _action = action;
            _arg1 = arg1;
            _arg2 = arg2;
        }

        public readonly EmptyStruct Execute(Context context, CancellationToken cancellationToken)
        {
            _action(context, cancellationToken, _arg1, _arg2);
            return EmptyStruct.Instance;
        }
    }

    /// <inheritdoc/>
    internal readonly struct SyncExecutableFuncNoParams<TResult> : ISyncExecutable<TResult>
    {
        private readonly Func<TResult> _func;

        /// <summary>
        /// Creates a <see cref="SyncExecutableFuncNoParams{TResult}"/> struct for the passed func, which may be executed through a policy at a later point in time.
        /// </summary>
        /// <param name="func">The function.</param>
        public SyncExecutableFuncNoParams(Func<TResult> func)
        {
            _func = func;
        }

        /// <inheritdoc/>
        public readonly TResult Execute(Context context, CancellationToken cancellationToken) => _func();
    }

    /// <inheritdoc/>
    internal readonly struct SyncExecutableFuncOnContext<TResult> : ISyncExecutable<TResult>
    {
        private readonly Func<Context, TResult> _func;

        /// <summary>
        /// Creates a <see cref="SyncExecutableFuncOnContext{TResult}"/> struct for the passed func, which may be executed through a policy at a later point in time.
        /// </summary>
        /// <param name="func">The function.</param>
        public SyncExecutableFuncOnContext(Func<Context, TResult> func)
        {
            _func = func;
        }

        /// <inheritdoc/>
        public readonly TResult Execute(Context context, CancellationToken cancellationToken) => _func(context);
    }

    /// <inheritdoc/>
    internal readonly struct SyncExecutableFuncOnCancellationToken<TResult> : ISyncExecutable<TResult>
    {
        private readonly Func<CancellationToken, TResult> _func;

        /// <summary>
        /// Creates a <see cref="SyncExecutableFuncOnCancellationToken{TResult}"/> struct for the passed func, which may be executed through a policy at a later point in time.
        /// </summary>
        /// <param name="func">The function.</param>
        public SyncExecutableFuncOnCancellationToken(Func<CancellationToken, TResult> func)
        {
            _func = func;
        }

        /// <inheritdoc/>
        public readonly TResult Execute(Context context, CancellationToken cancellationToken) => _func(cancellationToken);
    }

    /// <inheritdoc/>
    internal readonly struct SyncExecutableFunc<TResult> : ISyncExecutable<TResult>
    {
        private readonly Func<Context, CancellationToken, TResult> _func;

        /// <summary>
        /// Creates a <see cref="SyncExecutableFunc{TResult}"/> struct for the passed func, which may be executed through a policy at a later point in time.
        /// </summary>
        /// <param name="func">The function.</param>
        public SyncExecutableFunc(Func<Context, CancellationToken, TResult> func)
        {
            _func = func;
        }

        /// <inheritdoc/>
        public readonly TResult Execute(Context context, CancellationToken cancellationToken) => _func(context, cancellationToken);
    }

    /// <inheritdoc/>
    internal readonly struct SyncExecutableFunc<T1, TResult> : ISyncExecutable<TResult>
    {
        private readonly Func<Context, CancellationToken, T1, TResult> _func;
        private readonly T1 _arg1;

        public SyncExecutableFunc(Func<Context, CancellationToken, T1, TResult> func, T1 arg1)
        {
            _func = func;
            _arg1 = arg1;
        }

        /// <inheritdoc/>
        public readonly TResult Execute(Context context, CancellationToken cancellationToken) => _func(context, cancellationToken, _arg1);
    }

    /// <inheritdoc/>
    internal readonly struct SyncExecutableFunc<T1, T2, TResult> : ISyncExecutable<TResult>
    {
        private readonly Func<Context, CancellationToken, T1, T2, TResult> _func;
        private readonly T1 _arg1;
        private readonly T2 _arg2;

        public SyncExecutableFunc(Func<Context, CancellationToken, T1, T2, TResult> func, T1 arg1, T2 arg2)
        {
            _func = func;
            _arg1 = arg1;
            _arg2 = arg2;
        }

        /// <inheritdoc/>
        public readonly TResult Execute(Context context, CancellationToken cancellationToken) => _func(context, cancellationToken, _arg1, _arg2);
    }
}
