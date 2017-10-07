// ==================================
// This file only, code sourced from answer https://stackoverflow.com/a/31226509/ by https://stackoverflow.com/users/402022/theraot (Alfonso J. Ramos) to question https://stackoverflow.com/questions/20171877/ asked by https://stackoverflow.com/users/2674222/avo.  Also with contributions (as originally indicated by theraot within the code) by Miguel de Icaza (https://stackoverflow.com/users/16929/miguel-de-icaza).
// Licensed as described in and following the conditions of the StackExchange Terms Of Service (https://stackexchange.com/legal) (retrieved 23 July 2017) under the Creative Commons Attribution Share Alike license (https://creativecommons.org/licenses/by-sa/3.0/) (retrieved 23 July 2017).
// Minor amendment by @reisenberger (https://github.com/reisenberger/): removed unused private field _stackTraceOriginal.
// Code in this file (only) distributed under and governed by the Creative Commons Attribution Share Alike license (https://creativecommons.org/licenses/by-sa/3.0/).
// This notice must not be removed from this file.
// ==================================

#if NET40

using System;
using System.Text;
using System.Reflection;

namespace Polly.Utilities
{
    /// <summary>
    /// The ExceptionDispatchInfo object stores the stack trace information and Watson information that the exception contains at the point where it is captured. The exception can be thrown at another time and possibly on another thread by calling the ExceptionDispatchInfo.Throw method. The exception is thrown as if it had flowed from the point where it was captured to the point where the Throw method is called.
    /// </summary>
    public sealed class ExceptionDispatchInfo
    {
        private static FieldInfo _remoteStackTraceString;

        private readonly Exception _exception;
        private readonly object _stackTrace;

        private ExceptionDispatchInfo(Exception exception)
        {
            _exception = exception;
            _stackTrace = _exception.StackTrace;
            if (_stackTrace != null)
            {
                _stackTrace += Environment.NewLine + "---End of stack trace from previous location where exception was thrown ---" + Environment.NewLine;
            }
            else
            {
                _stackTrace = string.Empty;
            }
        }

        /// <summary>
        /// Creates an ExceptionDispatchInfo object that represents the specified exception at the current point in code.
        /// </summary>
        /// <param name="source">The exception whose state is captured, and which is represented by the returned object.</param>
        /// <returns>An object that represents the specified exception at the current point in code. </returns>
        public static ExceptionDispatchInfo Capture(Exception source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return new ExceptionDispatchInfo(source);
        }

        /// <summary>
        /// Gets the exception that is represented by the current instance.
        /// </summary>
        public Exception SourceException
        {
            get
            {
                return _exception;
            }
        }

        private static FieldInfo GetFieldInfo()
        {
            if (_remoteStackTraceString == null)
            {
                // ---
                // Code by Miguel de Icaza

                FieldInfo remoteStackTraceString =
                    typeof(Exception).GetField("_remoteStackTraceString",
                    BindingFlags.Instance | BindingFlags.NonPublic); // MS.Net

                if (remoteStackTraceString == null)
                    remoteStackTraceString = typeof(Exception).GetField("remote_stack_trace",
                        BindingFlags.Instance | BindingFlags.NonPublic); // Mono pre-2.6

                // ---
                _remoteStackTraceString = remoteStackTraceString;
            }
            return _remoteStackTraceString;
        }

        private static void SetStackTrace(Exception exception, object value)
        {
            FieldInfo remoteStackTraceString = GetFieldInfo();
            remoteStackTraceString.SetValue(exception, value);
        }

        /// <summary>
        /// Throws the exception that is represented by the current ExceptionDispatchInfo object, after restoring the state that was saved when the exception was captured.
        /// </summary>
        public void Throw()
        {
            try
            {
                throw _exception;
            }
            catch (Exception exception)
            {
                GC.KeepAlive(exception);
                var newStackTrace = _stackTrace + BuildStackTrace(Environment.StackTrace);
                SetStackTrace(_exception, newStackTrace);
                throw;
            }
        }

        private string BuildStackTrace(string trace)
        {
            var items = trace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var newStackTrace = new StringBuilder();
            var found = false;
            foreach (var item in items)
            {
                // Only include lines that has files in the source code
                if (item.Contains(":"))
                {
                    if (item.Contains("System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()"))
                    {
                        // Stacktrace from here on will be added by the CLR
                        break;
                    }
                    if (found)
                    {
                        newStackTrace.Append(Environment.NewLine);
                    }
                    found = true;
                    newStackTrace.Append(item);
                }
                else if (found)
                {
                    break;
                }
            }
            var result = newStackTrace.ToString();
            return result;
        }
    }
}

#endif