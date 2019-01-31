using System;
using System.Threading;

namespace Polly
{
    /// <summary>
    /// Implements elements common to both non-generic and generic policies, and sync and async policies.
    /// </summary>
    public abstract partial class PolicyBase
    {
        /// <summary>
        /// Predicates indicating which exceptions the policy handles.
        /// </summary>
        public ExceptionPredicates ExceptionPredicates { get; }

        /// <summary>
        /// Defines a CancellationToken to use, when none is supplied.
        /// </summary>
        internal readonly CancellationToken DefaultCancellationToken = CancellationToken.None;

        /// <summary>
        /// Defines a value to use for continueOnCaptureContext, when none is supplied.
        /// </summary>
        internal const bool DefaultContinueOnCapturedContext = false;

        internal static ExceptionType GetExceptionType(ExceptionPredicates exceptionPredicates, Exception exception)
        {
            bool isExceptionTypeHandledByThisPolicy = exceptionPredicates.FirstMatchOrDefault(exception) != null;

            return isExceptionTypeHandledByThisPolicy
                ? ExceptionType.HandledByThisPolicy
                : ExceptionType.Unhandled;
        }

        /// <summary>
        /// Constructs a new instance of a derived type of <see cref="PolicyBase"/> with the passed <paramref name="exceptionPredicates"/>.
        /// </summary>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
        protected PolicyBase(
            ExceptionPredicates exceptionPredicates)
        {
            ExceptionPredicates = exceptionPredicates ?? ExceptionPredicates.None;
        }
    }

    /// <summary>
    /// Implements elements common to sync and async generic policies.
    /// </summary>
    public abstract class PolicyBase<TResult> : PolicyBase
    {
        /// <summary>
        /// Predicates indicating which results the policy handles.
        /// </summary>
        public ResultPredicates<TResult> ResultPredicates { get; }

        /// <summary>
        /// Constructs a new instance of a derived type of <see cref="PolicyBase{TResult}"/> with the passed <paramref name="exceptionPredicates"/> and <paramref name="resultPredicates"/>.
        /// </summary>
        /// <param name="exceptionPredicates">Predicates indicating which exceptions the policy should handle. </param>
        /// <param name="resultPredicates">Predicates indicating which results the policy should handle. </param>
        protected PolicyBase(
            ExceptionPredicates exceptionPredicates,
            ResultPredicates<TResult> resultPredicates)
        : base(exceptionPredicates)
        {
            ResultPredicates = resultPredicates ?? ResultPredicates<TResult>.None;
        }
}
}
