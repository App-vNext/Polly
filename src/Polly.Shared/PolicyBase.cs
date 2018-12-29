﻿using System;
using System.Threading;

namespace Polly
{
    /// <summary>
    /// Implements elements common to both non-generic and generic policies, and sync and async policies.
    /// </summary>
    public abstract partial class PolicyBase
    {
        /// <summary>
        /// Predicates indicating which exceptions the policy should handle.
        /// </summary>
        internal ExceptionPredicates ExceptionPredicates { get; set; }

        /// <summary>
        /// Defines a CancellationToken to use, when none is supplied.
        /// </summary>
        internal CancellationToken DefaultCancellationToken = CancellationToken.None;

        /// <summary>
        /// Defines a value to use for continueOnCaptureContext, when none is supplied.
        /// </summary>
        internal bool DefaultContinueOnCapturedContext = false;

        internal static ExceptionType GetExceptionType(ExceptionPredicates exceptionPredicates, Exception exception)
        {
            bool isExceptionTypeHandledByThisPolicy = exceptionPredicates.FirstMatchOrDefault(exception) != null;

            return isExceptionTypeHandledByThisPolicy
                ? ExceptionType.HandledByThisPolicy
                : ExceptionType.Unhandled;
        }

    }
}
