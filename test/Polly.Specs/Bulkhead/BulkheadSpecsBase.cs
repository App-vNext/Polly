namespace Polly.Specs.Bulkhead;

[Collection(Constants.ParallelThreadDependentTestCollection)]
public abstract class BulkheadSpecsBase : IDisposable
{
    protected static AssertionFailure? Expect(int expected, Func<int> actualFunc, string measure)
    {
        int actual = actualFunc();
        return actual != expected ? new AssertionFailure(expected, actual, measure) : null;
    }

    #region Time constraints

    protected readonly TimeSpan ShimTimeSpan = TimeSpan.FromMilliseconds(50); // How frequently to retry the assertions.
    protected readonly TimeSpan CohesionTimeLimit = TimeSpan.FromSeconds(1);

    #endregion

    protected static CancellationToken CancellationToken => CancellationToken.None;

    protected BulkheadSpecsBase(ITestOutputHelper testOutputHelper)
    {
#if DEBUG
        TestOutputHelper = new AnnotatedOutputHelper(testOutputHelper);
#else
        TestOutputHelper = new SilentOutputHelper();
#endif

        ThreadPool.SetMinThreads(50, 20);
    }

    #region Operating variables

    protected IBulkheadPolicy BulkheadForStats { get; set; } = null!;

    internal TraceableAction[] Actions { get; set; } = [];

    protected Task[] Tasks { get; set; } = [];

    protected readonly AutoResetEvent StatusChangedEvent = new(false);

    #endregion

    #region Scenario

    protected string? Scenario { get; set; }

    protected int MaxParallelization { get; set; }
    protected int MaxQueuingActions { get; set; }
    protected int TotalActions { get; set; }

    #endregion

    #region Tracked metrics

    protected int ExpectedCompleted { get; set; }
    protected int ExpectedCancelled { get; set; }
    protected int ExpectedExecuting { get; set; }
    protected int ExpectedRejects { get; set; }
    protected int ExpectedQueuing { get; set; }
    protected int ExpectedFaulted { get; set; }
    protected int ExpectedBulkheadFree { get; set; }
    protected int ExpectedQueueFree { get; set; }

    protected int ActualCompleted { get; set; }
    protected int ActualCancelled { get; set; }
    protected int ActualExecuting { get; set; }
    protected int ActualRejects { get; set; }
    protected int ActualQueuing { get; set; }
    protected int ActualFaulted { get; set; }
    protected int ActualBulkheadFree => BulkheadForStats.BulkheadAvailableCount;

    protected int ActualQueueFree => BulkheadForStats.QueueAvailableCount;

    #endregion

    #region Bulkhead behaviour

    protected abstract IBulkheadPolicy GetBulkhead(int maxParallelization, int maxQueuingActions);

    protected abstract Task ExecuteOnBulkhead(IBulkheadPolicy bulkhead, TraceableAction action);

    [Theory]
    [ClassData(typeof(BulkheadScenarios))]
    public void Should_control_executions_per_specification(int maxParallelization, int maxQueuingActions, int totalActions, bool cancelQueuing, bool cancelExecuting, string scenario)
    {
        totalActions.ShouldBeGreaterThanOrEqualTo(0);

        MaxParallelization = maxParallelization;
        MaxQueuingActions = maxQueuingActions;
        TotalActions = totalActions;
        Scenario = $"MaxParallelization {maxParallelization}; MaxQueuing {maxQueuingActions}; TotalActions {totalActions}; CancelQueuing {cancelQueuing}; CancelExecuting {cancelExecuting}: {scenario}";

        IBulkheadPolicy bulkhead = GetBulkhead(maxParallelization, maxQueuingActions);
        using (bulkhead)
        {
            BulkheadForStats = bulkhead;

            // Set up delegates which we can track whether they've started; and control when we allow them to complete (to release their semaphore slot).
            Actions = new TraceableAction[totalActions];
            for (int i = 0; i < totalActions; i++)
            {
                Actions[i] = new TraceableAction(i, StatusChangedEvent, TestOutputHelper);
            }

            // Throw all the delegates at the bulkhead simultaneously.
            Tasks = new Task[totalActions];
            for (int i = 0; i < totalActions; i++)
            {
                Tasks[i] = ExecuteOnBulkhead(bulkhead, Actions[i]);
            }

            OutputStatus("Immediately after queueing...");

            // Assert the expected distributions of executing, queuing, rejected and completed - when all delegates thrown at bulkhead.
            ExpectedCompleted = 0;
            ExpectedCancelled = 0;
            ExpectedExecuting = Math.Min(totalActions, maxParallelization);
            ExpectedRejects = Math.Max(0, totalActions - maxParallelization - maxQueuingActions);
            ExpectedQueuing = Math.Min(maxQueuingActions, Math.Max(0, totalActions - maxParallelization));
            ExpectedBulkheadFree = maxParallelization - ExpectedExecuting;
            ExpectedQueueFree = maxQueuingActions - ExpectedQueuing;

            try
            {
                Within(CohesionTimeLimit, ActualsMatchExpecteds);
            }
            finally
            {
                OutputStatus("Expected initial state verified...");
            }

            // Complete or cancel delegates one by one, and expect others to take their place (if a slot released and others remain queueing); until all work is done.
            while (ExpectedExecuting > 0)
            {
                if (cancelQueuing)
                {
                    TestOutputHelper.WriteLine("Cancelling a queueing task...");

                    Actions.First(a => a.Status == TraceableActionStatus.QueueingForSemaphore).Cancel();

                    ExpectedCancelled++;
                    ExpectedQueuing--;
                    ExpectedQueueFree++;

                    cancelQueuing = false;
                }
                else if (cancelExecuting)
                {
                    TestOutputHelper.WriteLine("Cancelling an executing task...");

                    Actions.First(a => a.Status == TraceableActionStatus.Executing).Cancel();

                    ExpectedCancelled++;
                    if (ExpectedQueuing > 0)
                    {
                        ExpectedQueuing--;
                        ExpectedQueueFree++;
                    }
                    else
                    {
                        ExpectedExecuting--;
                        ExpectedBulkheadFree++;
                    }

                    cancelExecuting = false;
                }
                else
                {
                    // Complete an executing delegate.
                    TestOutputHelper.WriteLine("Completing a task...");

                    Actions.First(a => a.Status == TraceableActionStatus.Executing).AllowCompletion();

                    ExpectedCompleted++;

                    if (ExpectedQueuing > 0)
                    {
                        ExpectedQueuing--;
                        ExpectedQueueFree++;
                    }
                    else
                    {
                        ExpectedExecuting--;
                        ExpectedBulkheadFree++;
                    }

                }

                try
                {
                    Within(CohesionTimeLimit, ActualsMatchExpecteds);
                }
                finally
                {
                    OutputStatus("End of next loop iteration...");
                }

            }

            EnsureNoUnbservedTaskExceptions();

            TestOutputHelper.WriteLine("Verifying all tasks completed...");
            Within(CohesionTimeLimit, AllTasksCompleted);
        }
    }

    protected void UpdateActuals()
    {
        ActualCompleted = ActualCancelled = ActualExecuting = ActualRejects = ActualQueuing = ActualFaulted = 0;

        foreach (TraceableAction action in Actions)
        {
            switch (action.Status)
            {
                case TraceableActionStatus.Canceled:
                    ActualCancelled++;
                    break;
                case TraceableActionStatus.Completed:
                    ActualCompleted++;
                    break;
                case TraceableActionStatus.Executing:
                    ActualExecuting++;
                    break;
                case TraceableActionStatus.Faulted:
                    ActualFaulted++;
                    break;
                case TraceableActionStatus.QueueingForSemaphore:
                    ActualQueuing++;
                    break;
                case TraceableActionStatus.Rejected:
                    ActualRejects++;
                    break;
                case TraceableActionStatus.StartRequested:
                case TraceableActionStatus.Unstarted:
                    // We do not care to count these.
                    break;
                default:
                    throw new InvalidOperationException($"Unaccounted for {nameof(TraceableActionStatus)}: {action.Status}.");
            }
        }
    }

    protected AssertionFailure? ActualsMatchExpecteds()
    {
        UpdateActuals();

        if (ExpectedFaulted != ActualFaulted)
        {
            return new AssertionFailure(ExpectedFaulted, ActualFaulted, nameof(ExpectedFaulted));
        }

        if (ExpectedRejects != ActualRejects)
        {
            return new AssertionFailure(ExpectedRejects, ActualRejects, nameof(ExpectedRejects));
        }

        if (ExpectedCancelled != ActualCancelled)
        {
            return new AssertionFailure(ExpectedCancelled, ActualCancelled, nameof(ExpectedCancelled));
        }

        if (ExpectedCompleted != ActualCompleted)
        {
            return new AssertionFailure(ExpectedCompleted, ActualCompleted, nameof(ExpectedCompleted));
        }

        if (ExpectedExecuting != ActualExecuting)
        {
            return new AssertionFailure(ExpectedExecuting, ActualExecuting, nameof(ExpectedExecuting));
        }

        if (ExpectedQueuing != ActualQueuing)
        {
            return new AssertionFailure(ExpectedQueuing, ActualQueuing, nameof(ExpectedQueuing));
        }

        if (ExpectedBulkheadFree != ActualBulkheadFree)
        {
            return new AssertionFailure(ExpectedBulkheadFree, ActualBulkheadFree, nameof(ExpectedBulkheadFree));
        }

        if (ExpectedQueueFree != ActualQueueFree)
        {
            return new AssertionFailure(ExpectedQueueFree, ActualQueueFree, nameof(ExpectedQueueFree));
        }

        return null;
    }

    protected AssertionFailure? AllTasksCompleted()
    {
        int countTasksCompleted = Tasks.Count(t => t.IsCompleted);
        if (countTasksCompleted < TotalActions)
        {
            return new AssertionFailure(TotalActions, countTasksCompleted, nameof(countTasksCompleted));
        }

        return null;
    }

    protected void EnsureNoUnbservedTaskExceptions()
    {
        for (int i = 0; i < Tasks.Length; i++)
        {
            try
            {
                Tasks[i].Wait();
            }
            catch (Exception e)
            {
                throw new Exception($"Task {i} raised the following unobserved task exception: ", e);
            }
        }
    }

    #endregion

    protected void Within(TimeSpan timeSpan, Func<AssertionFailure?> actionContainingAssertions)
    {
        TimeSpan permitted = timeSpan;
        Stopwatch watch = Stopwatch.StartNew();
        while (true)
        {
            var potentialFailure = actionContainingAssertions();
            if (potentialFailure == null)
            {
                break;
            }

            if (watch.Elapsed > permitted)
            {
                TestOutputHelper.WriteLine("Failing assertion on: {0}", potentialFailure.Measure);
                potentialFailure.Actual.ShouldBe(potentialFailure.Expected, $"for '{potentialFailure.Measure}', in scenario: {Scenario}");
                throw new InvalidOperationException("Code should never reach here. Preceding assertion should fail.");
            }

            bool signaled = StatusChangedEvent.WaitOne(ShimTimeSpan);
            if (signaled)
            {
                // Following TraceableAction.CaptureCompletion() signalling the AutoResetEvent,
                // there can be race conditions between on the one hand exiting the bulkhead semaphore (and potentially another execution gaining it),
                // and the assertion being verified here about those same facts.
                // If that race is lost by the real-world state change, and the AutoResetEvent signal occurred very close to timeoutTime,
                // there might not be a second chance.
                // We therefore permit another shim time for the condition to come good.
                permitted += CohesionTimeLimit;
            }
        }
    }

    #region Output helpers

    protected readonly ITestOutputHelper TestOutputHelper;

    protected void OutputStatus(string statusHeading)
    {
        TestOutputHelper.WriteLine(statusHeading);
        TestOutputHelper.WriteLine("Bulkhead: {0} slots out of {1} available.", ActualBulkheadFree, MaxParallelization);
        TestOutputHelper.WriteLine("Bulkhead queue: {0} slots out of {1} available.", ActualQueueFree, MaxQueuingActions);

        for (int i = 0; i < Actions.Length; i++)
        {
            TestOutputHelper.WriteLine("Action {0}: {1}", i, Actions[i].Status);
        }

        TestOutputHelper.WriteLine(string.Empty);
    }
#if DEBUG
    private void ShowTestOutput() =>
        ((AnnotatedOutputHelper)TestOutputHelper).Flush();
#endif

    #endregion

    public void Dispose()
    {
#if DEBUG
        ShowTestOutput();
#endif

        if (Actions != null)
        {
            foreach (TraceableAction action in Actions)
            {
                action.Dispose();
            }
        }

        StatusChangedEvent.Dispose();
    }
}
