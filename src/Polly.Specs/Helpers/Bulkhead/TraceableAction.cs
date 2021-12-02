using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Bulkhead;
using Xunit.Abstractions;

namespace Polly.Specs.Helpers.Bulkhead
{
    /// <summary>
    /// A traceable action that can be executed on a <see cref="BulkheadPolicy"/>, to support specs.
    /// <remarks>We can execute multiple instances of <see cref="TraceableAction"/> in parallel on a bulkhead, and manually control the cancellation and completion of each, to provide determinate tests on the bulkhead operation.  The status of this <see cref="TraceableAction"/> as it executes is fully traceable through the <see cref="TraceableActionStatus"/> property.</remarks>
    /// </summary>
    public class TraceableAction : IDisposable
    {
        private readonly string _id;
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly TaskCompletionSource<object> _tcsProxyForRealWork = new TaskCompletionSource<object>();
        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        private TraceableActionStatus _status;
        private readonly AutoResetEvent _statusChanged;

        public TraceableActionStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                _testOutputHelper.WriteLine(_id + "Updated status to {0}, signalling AutoResetEvent.", _status);
                SignalStateChange();
            }
        }

        public TraceableAction(int id, AutoResetEvent statusChanged, ITestOutputHelper testOutputHelper)
        {
            _id = $"{id:00}: ";
            _statusChanged = statusChanged;
            _testOutputHelper = testOutputHelper;
        }

        public void SignalStateChange()
        {
            _testOutputHelper.WriteLine("--signalled--");
            _statusChanged.Set();
        }

        public Task ExecuteOnBulkhead(BulkheadPolicy bulkhead)
        {
            return ExecuteThroughSyncBulkheadOuter(
                () => bulkhead.Execute(_ => ExecuteThroughSyncBulkheadInner(), CancellationSource.Token)
                );
        }

        public Task ExecuteOnBulkhead<TResult>(BulkheadPolicy<TResult> bulkhead)
        {
            return ExecuteThroughSyncBulkheadOuter(
                () => bulkhead.Execute(_ => { ExecuteThroughSyncBulkheadInner(); return default; }, CancellationSource.Token)
                );
        }

        // Note re TaskCreationOptions.LongRunning: Testing the parallelization of the bulkhead policy efficiently requires the ability to start large numbers of parallel tasks in a short space of time.  The ThreadPool's algorithm of only injecting extra threads (when necessary) at a rate of two-per-second however makes high-volume tests using the ThreadPool both slow and flaky.  For PCL tests further, ThreadPool.SetMinThreads(...) is not available, to mitigate this.  Using TaskCreationOptions.LongRunning allows us to force tasks to be started near-instantly on non-ThreadPool threads.
        private Task ExecuteThroughSyncBulkheadOuter(Action executeThroughBulkheadInner)
        {
            if (Status != TraceableActionStatus.Unstarted)
            {
                throw new InvalidOperationException(_id + "Action has previously been started.");
            }
            Status = TraceableActionStatus.StartRequested;

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    Status = TraceableActionStatus.QueueingForSemaphore;

                    executeThroughBulkheadInner();
                }
                catch (BulkheadRejectedException)
                {
                    Status = TraceableActionStatus.Rejected;
                }
                catch (OperationCanceledException)
                {
                    if (Status != TraceableActionStatus.Canceled)
                    {
                        _testOutputHelper.WriteLine(_id + "Caught queue cancellation.");
                        Status = TraceableActionStatus.Canceled;
                    }  // else: was execution cancellation rethrown: ignore
                }
                catch (AggregateException ae)
                {
                    if (ae.InnerExceptions.Count == 1 && ae.InnerException is OperationCanceledException)
                    {
                        if (Status != TraceableActionStatus.Canceled)
                        {
                            _testOutputHelper.WriteLine(_id + "Caught queue cancellation.");
                            Status = TraceableActionStatus.Canceled;
                        } // else: was execution cancellation rethrown: ignore
                    }
                    else throw;
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine(_id + "Caught unexpected exception during execution: " + e);

                    Status = TraceableActionStatus.Faulted;
                }
                finally
                {
                    // Exiting the execution successfully is also a change of state (on which assertions may be occurring) in that it releases a semaphore slot sucessfully.
                    // There can also be races between assertions and executions-responding-to-previous-state-changes, so a second signal presents another opportunity for assertions to be run.
                    SignalStateChange();
                }
            },
            TaskCreationOptions.LongRunning);
        }

        private void ExecuteThroughSyncBulkheadInner()
        {
            Status = TraceableActionStatus.Executing;

            _tcsProxyForRealWork.Task.ContinueWith(CaptureCompletion(), TaskContinuationOptions.ExecuteSynchronously).Wait();

            _testOutputHelper.WriteLine(_id + "Exiting execution.");
        }

        public Task ExecuteOnBulkheadAsync(AsyncBulkheadPolicy bulkhead)
        {
            return ExecuteThroughAsyncBulkheadOuter(
                () => bulkhead.ExecuteAsync(async _ => await ExecuteThroughAsyncBulkheadInner(), CancellationSource.Token)
            );
        }

        public Task ExecuteOnBulkheadAsync<TResult>(AsyncBulkheadPolicy<TResult> bulkhead)
        {
            return ExecuteThroughAsyncBulkheadOuter(
                () => bulkhead.ExecuteAsync(async _ => { await ExecuteThroughAsyncBulkheadInner(); return default; }, CancellationSource.Token)
            );
        }

        public Task ExecuteThroughAsyncBulkheadOuter(Func<Task> executeThroughBulkheadInner)
        {
            if (Status != TraceableActionStatus.Unstarted)
            {
                throw new InvalidOperationException(_id + "Action has previously been started.");
            }
            Status = TraceableActionStatus.StartRequested;

            return Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        Status = TraceableActionStatus.QueueingForSemaphore;

                        await executeThroughBulkheadInner();
                    }
                    catch (BulkheadRejectedException)
                    {
                        Status = TraceableActionStatus.Rejected;
                    }
                    catch (OperationCanceledException)
                    {
                        if (Status != TraceableActionStatus.Canceled)
                        {
                            _testOutputHelper.WriteLine(_id + "Caught queue cancellation.");
                            Status = TraceableActionStatus.Canceled;
                        } // else: was execution cancellation rethrown: ignore
                    }
                    catch (Exception e)
                    {
                        _testOutputHelper.WriteLine(_id + "Caught unexpected exception during execution: " + e);

                        Status = TraceableActionStatus.Faulted;
                    }
                    finally
                    {
                        // Exiting the execution successfully is also a change of state (on which assertions may be occurring) in that it releases a semaphore slot sucessfully.
                        // There can also be races between assertions and executions-responding-to-previous-state-changes, so a second signal presents another opportunity for assertions to be run.
                        SignalStateChange();
                    }
                },
                TaskCreationOptions.LongRunning).Unwrap();
        }

        private async Task ExecuteThroughAsyncBulkheadInner()
        {
            Status = TraceableActionStatus.Executing;

            await _tcsProxyForRealWork.Task.ContinueWith(CaptureCompletion(), TaskContinuationOptions.ExecuteSynchronously);

            _testOutputHelper.WriteLine(_id + "Exiting execution.");
        }

        private Action<Task<object>> CaptureCompletion() => t =>
        {
            if (t.IsCanceled)
            {
                _testOutputHelper.WriteLine(_id + "Cancelling execution.");

                Status = TraceableActionStatus.Canceled;
                throw new OperationCanceledException(CancellationSource.Token); // Exception rethrown for the purpose of testing exceptions thrown through the BulkheadEngine.
            }
            else if (t.IsFaulted)
            {
                _testOutputHelper.WriteLine(_id + "Execution faulted.");
                if (t.Exception != null) { _testOutputHelper.WriteLine(_id + "Exception: " + t.Exception); }

                Status = TraceableActionStatus.Faulted;
            }
            else
            {
                _testOutputHelper.WriteLine(_id + "Completing execution.");

                Status = TraceableActionStatus.Completed;
            }

        };

        public void AllowCompletion()
        {
            _tcsProxyForRealWork.SetResult(null);
        }

        public void Cancel()
        {
            if (CancellationSource.IsCancellationRequested) { throw new InvalidOperationException(_id + "Action has already been cancelled."); }
            CancellationSource.Cancel();

            _tcsProxyForRealWork.SetCanceled();
        }

        public void Dispose()
        {
            CancellationSource.Dispose();
        }
    }
}
