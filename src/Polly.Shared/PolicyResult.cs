using System;

namespace Polly
{
    /// <summary>
    /// The captured result of executing a policy
    /// </summary>
    public class PolicyResult
    {
        private readonly OutcomeType _outcome;
        private readonly Exception _finalException;
        private readonly ExceptionType? _exceptionType;

        internal PolicyResult(OutcomeType outcome, Exception finalException, ExceptionType? exceptionType)
        {
            _outcome = outcome;
            _finalException = finalException;
            _exceptionType = exceptionType;
        }

        /// <summary>
        ///   The outcome of executing the policy
        /// </summary>
        public OutcomeType Outcome
        {
            get { return _outcome; }
        }

        /// <summary>
        ///  The final exception captured. Will be null if policy executed successfully
        /// </summary>
        public Exception FinalException
        {
            get { return _finalException; }
        }

        /// <summary>
        ///  The exception type of the final exception captured. Will be null if policy executed successfully
        /// </summary>
        public ExceptionType? ExceptionType
        {
            get { return _exceptionType; }
        }

        internal static PolicyResult Successful()
        {
            return new PolicyResult(OutcomeType.Successful, null, null);
        }

        internal static PolicyResult Failure(Exception exception, ExceptionType exceptionType)
        {
            return new PolicyResult(OutcomeType.Failure, exception, exceptionType);
        }
    }

    /// <summary>
    /// The captured result of executing a policy
    /// </summary>
    public class PolicyResult<TResult>
    {
        private readonly TResult _result;
        private readonly OutcomeType _outcome;
        private readonly Exception _finalException;
        private readonly ExceptionType? _exceptionType;

        internal PolicyResult(TResult result, OutcomeType outcome, Exception finalException, ExceptionType? exceptionType)
        {
            _result = result;
            _outcome = outcome;
            _finalException = finalException;
            _exceptionType = exceptionType;
        }

        /// <summary>
        ///   The outcome of executing the policy
        /// </summary>
        public OutcomeType Outcome
        {
            get { return _outcome; }
        }

        /// <summary>
        ///  The final exception captured. Will be null if policy executed successfully
        /// </summary>
        public Exception FinalException
        {
            get { return _finalException; }
        }

        /// <summary>
        ///  The exception type of the final exception captured. Will be null if policy executed successfully
        /// </summary>
        public ExceptionType? ExceptionType
        {
            get { return _exceptionType; }
        }

        /// <summary>
        /// The result of executing the policy. Will be default(TResult) is the policy failed
        /// </summary>
        public TResult Result
        {
            get { return _result; }
        }

        internal static PolicyResult<TResult> Successful(TResult result)
        {
            return new PolicyResult<TResult>(result, OutcomeType.Successful, null, null);
        }

        internal static PolicyResult<TResult> Failure(Exception exception, ExceptionType exceptionType)
        {
            return new PolicyResult<TResult>(default(TResult), OutcomeType.Failure, exception, exceptionType);
        }
    }

    /// <summary>
    /// Represents the outcome of executing a policy
    /// </summary>
    public enum OutcomeType
    {
        /// <summary>
        /// Indicates that the policy ultimately executed successfully
        /// </summary>
        Successful,

        /// <summary>
        /// Indicates that the policy ultimately failed
        /// </summary>
        Failure
    };

    /// <summary>
    /// Represents the type of exception resulting from a failed policy
    /// </summary>
    public enum ExceptionType
    {
        /// <summary>
        /// An exception type that has been defined to be handled by this policy
        /// </summary>
        HandledByThisPolicy,

        /// <summary>
        /// An exception type that has been not been defined to be handled by this policy
        /// </summary>
        Unhandled
    }
}