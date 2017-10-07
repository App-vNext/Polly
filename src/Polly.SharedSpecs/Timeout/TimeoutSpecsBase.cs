using System;

namespace Polly.Specs.Timeout
{
    public class TimeoutSpecsBase
    {
        /// <summary>
        /// A helper method which simply throws the passed exception.  Supports tests verifying the stack trace of where an exception was thrown.
        /// </summary>
        /// <param name="ex">The exception to throw.</param>
        protected void Helper_ThrowException(Exception ex)
        {
            throw ex;
        }
    }
}
