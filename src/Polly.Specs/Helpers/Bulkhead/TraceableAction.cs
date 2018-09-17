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
    internal class TraceableAction : IDisposable
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
                _statusChanged.Set();
            }
        }

        public TraceableAction(int id, AutoResetEvent statusChanged, ITestOutputHelper testOutputHelper)
        {
            _id = String.Format("{0:00}", id) + ": ";
            _statusChanged = statusChanged;
            _testOutputHelper = testOutputHelper;
        }

    // Note re TaskCreationOptions.LongRunning: Testing the parallelization of the bulkhead policy efficiently requires the ability to start large numbers of parallel tasks in a short space of time.  The ThreadPool's algorithm of only injecting extra threads (when necessary) at a rate of two-per-second however makes high-volume tests using the ThreadPool both slow and flaky.  For PCL tests further, ThreadPool.SetMinThreads(...) is not available, to mitigate this.  Using TaskCreationOptions.LongRunning allows us to force tasks to be started near-instantly on non-ThreadPool threads.
    // Similarly, we use ConfigureAwait(true) when awaiting, to avoid continuations being scheduled onto a ThreadPool thread, which may only be injected too slowly in high-volume tests.

        public Task ExecuteOnBulkhead(BulkheadPolicy bulkhead)
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

                    bulkhead.Execute(ct =>
                    {
                        Status = TraceableActionStatus.Executing;

                        _tcsProxyForRealWork.Task.ContinueWith(CaptureCompletion()).Wait();

                        _testOutputHelper.WriteLine(_id + "Exiting execution.");
                    }, CancellationSource.Token);
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
            }, 
            TaskCreationOptions.LongRunning); 
        }

        public Task<TResult> ExecuteOnBulkhead<TResult>(BulkheadPolicy<TResult> bulkhead)
        {
            if (Status != TraceableActionStatus.Unstarted)
            {
                throw new InvalidOperationException(_id + "Action has previously been started.");
            }
            Status = TraceableActionStatus.StartRequested;

            TResult result = default(TResult);
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    Status = TraceableActionStatus.QueueingForSemaphore; 

                    result = bulkhead.Execute(ct =>
                    {
                        Status = TraceableActionStatus.Executing;

                        _tcsProxyForRealWork.Task.ContinueWith(CaptureCompletion()).Wait();

                        _testOutputHelper.WriteLine(_id + "Exiting execution.");

                        return default(TResult);
                    }, CancellationSource.Token);
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
                return result;
            }, 
            TaskCreationOptions.LongRunning);

        }

        public Task ExecuteOnBulkheadAsync(BulkheadPolicy bulkhead)
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

                    await bulkhead.ExecuteAsync(async ct =>
                    {
                        Status = TraceableActionStatus.Executing;

                        await _tcsProxyForRealWork.Task.ContinueWith(CaptureCompletion()).ConfigureAwait(true);

                        _testOutputHelper.WriteLine(_id + "Exiting execution.");

                    }, CancellationSource.Token).ConfigureAwait(true);
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
            }, 
            TaskCreationOptions.LongRunning).Unwrap();
        }

        public Task<TResult> ExecuteOnBulkheadAsync<TResult>(BulkheadPolicy<TResult> bulkhead)
        {
            if (Status != TraceableActionStatus.Unstarted)
            {
                throw new InvalidOperationException(_id + "Action has previously been started.");
            }
            Status = TraceableActionStatus.StartRequested;

            TResult result = default(TResult);
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    Status = TraceableActionStatus.QueueingForSemaphore;

                    result = await bulkhead.ExecuteAsync(async ct =>
                    {
                        Status = TraceableActionStatus.Executing;

                        await _tcsProxyForRealWork.Task.ContinueWith(CaptureCompletion()).ConfigureAwait(true);

                        _testOutputHelper.WriteLine(_id + "Exiting execution.");

                        return default(TResult);
                    }, CancellationSource.Token).ConfigureAwait(true); 
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
                return result;
            }
            , TaskCreationOptions.LongRunning).Unwrap();
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

    /// <summary>
    /// States of a <see cref="TraceableAction"/> that can be tracked during testing.
    /// </summary>
    internal enum TraceableActionStatus
    {
        Unstarted,
        StartRequested,
        QueueingForSemaphore,
        Executing,
        Rejected,
        Canceled,
        Faulted,
        Completed,
    }
}
