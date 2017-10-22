using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using Polly.Specs.Helpers.Bulkhead;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Polly.Specs.Bulkhead
{
    public class BulkheadSpecsHelper : IDisposable
    {
        protected ITestOutputHelper testOutputHelper;

        protected TimeSpan shimTimeSpan = TimeSpan.FromMilliseconds(2000); // Consider increasing shimTimeSpan if bulkhead specs fail transiently in slower build environments.

        internal TraceableAction[] actions;

        protected readonly AutoResetEvent statusChanged = new AutoResetEvent(false);

        public BulkheadSpecsHelper(ITestOutputHelper testOutputHelper)
        {
#if !DEBUG 
            testOutputHelper = new SilentOutput();
#endif
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Asserts that the actionContainingAssertions will succeed without <see cref="AssertionFailedException"/> or <see cref="XunitException"/>, within the given timespan.  Checks are made each time a status-change pulse is received from the <see cref="TraceableAction"/>s executing through the bulkhead.
        /// </summary>
        /// <param name="timeSpan">The allowable timespan.</param>
        /// <param name="actionContainingAssertions">The action containing fluent assertions, which must succeed within the timespan.</param>
        protected void Within(TimeSpan timeSpan, Action actionContainingAssertions)
        {
            DateTime timeoutTime = DateTime.UtcNow.Add(timeSpan);
            while (true)
            {
                try
                {
                    actionContainingAssertions.Invoke();
                    break;
                }
                catch (Exception e)
                {
                    if (!(e is AssertionFailedException || e is XunitException)) { throw; }

                    TimeSpan remaining = timeoutTime - DateTime.UtcNow;
                    if (remaining <= TimeSpan.Zero) { throw; }

                    statusChanged.WaitOne(remaining);
                }
            }
        }

        protected void OutputActionStatuses()
        {
            for (int i = 0; i < actions.Length; i++)
            {
                testOutputHelper.WriteLine("Action {0}: {1}", i, actions[i].Status);
            }
            testOutputHelper.WriteLine(String.Empty);
        }

        protected static void EnsureNoUnbservedTaskExceptions(Task[] tasks)
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                try
                {
                    tasks[i].Wait();
                }
                catch (Exception e)
                {
                    throw new Exception("Task " + i + " raised the following unobserved task exception: ", e);
                }
            }

        }

        public void Dispose()
        {
            if (actions != null)
            {
                foreach (TraceableAction action in actions)
                {
                    action.Dispose();
                }
            }
        }

#if !DEBUG
        public class SilentOutput : ITestOutputHelper
        {
            public void WriteLine(string message)
            {
                // Do nothing: intentionally silent.
            }

            public void WriteLine(string format, params object[] args)
            {
                // Do nothing: intentionally silent.
            }
        }
#endif

    }
}
