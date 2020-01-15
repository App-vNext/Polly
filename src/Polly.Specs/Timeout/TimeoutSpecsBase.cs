using System;
using System.Runtime.CompilerServices;
using Polly.Utilities;

namespace Polly.Specs.Timeout
{
    /// <summary>
    /// Provides common functionality for timeout specs, which abstracts out both SystemClock.Sleep, and SystemClock.CancelTokenAfter.
    /// <remarks>Polly's TimeoutPolicy uses timing-out CancellationTokens to drive timeouts.
    /// For tests, rather than letting .NET's timers drive the timing out of CancellationTokens, we override SystemClock.CancelTokenAfter and SystemClock.Sleep to make the tests run fast.
    /// </remarks>
    /// </summary>
    public abstract class TimeoutSpecsBase : IDisposable
    {
        protected TimeoutSpecsBase()
        {
            SystemClock.Current = new TestSystemClock();
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }

        /// <summary>
        /// A helper method which simply throws the passed exception.  Supports tests verifying the stack trace of where an exception was thrown, by throwing that exception from a specific (other) location.
        /// </summary>
        /// <param name="ex">The exception to throw.</param>
        [MethodImpl(MethodImplOptions.NoInlining)] // Tests that use this method assert that the exception was thrown from within this method; therefore, it is essential that 
        protected void Helper_ThrowException(Exception ex)
        {
            throw ex;
        }
    }
}
