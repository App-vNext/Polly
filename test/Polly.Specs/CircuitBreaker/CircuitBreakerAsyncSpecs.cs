using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.CircuitBreaker;

[Collection(Constants.SystemClockDependentTestCollection)]
public class CircuitBreakerAsyncSpecs : IDisposable
{
    #region Configuration tests

    [Fact]
    public async Task Should_be_able_to_handle_a_duration_of_timespan_maxvalue()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, TimeSpan.MaxValue);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public void Should_throw_if_exceptions_allowed_before_breaking_is_less_than_one()
    {
        Action action = () => Policy
                                 .Handle<DivideByZeroException>()
                                 .CircuitBreakerAsync(0, TimeSpan.FromSeconds(10));

        Should.Throw<ArgumentOutOfRangeException>(action)
              .ParamName.ShouldBe("exceptionsAllowedBeforeBreaking");
    }

    [Fact]
    public void Should_throw_if_duration_of_break_is_less_than_zero()
    {
        Action action = () => Policy
                                 .Handle<DivideByZeroException>()
                                 .CircuitBreakerAsync(1, -TimeSpan.FromSeconds(1));

        Should.Throw<ArgumentOutOfRangeException>(action)
            .ParamName.ShouldBe("durationOfBreak");
    }

    [Fact]
    public void Should_be_able_to_handle_a_duration_of_break_of_zero()
    {
        Action action = () => Policy
                                 .Handle<DivideByZeroException>()
                                 .CircuitBreakerAsync(1, TimeSpan.Zero);
        Should.NotThrow(action);
    }

    [Fact]
    public void Should_initialise_to_closed_state()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    #endregion

    #region Circuit-breaker threshold-to-break tests

    [Fact]
    public async Task Should_not_open_circuit_if_specified_number_of_specified_exception_are_not_raised_consecutively()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_after_specified_number_of_specified_exception_have_been_raised()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        bool delegateExecutedWhenBroken = false;
        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }));
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        delegateExecutedWhenBroken.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_after_specified_number_of_one_of_the_specified_exceptions_have_been_raised()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .Or<ArgumentOutOfRangeException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => breaker.RaiseExceptionAsync<ArgumentOutOfRangeException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        bool delegateExecutedWhenBroken = false;
        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }));
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<ArgumentOutOfRangeException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        delegateExecutedWhenBroken.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_open_circuit_if_exception_raised_is_not_the_specified_exception()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await Should.ThrowAsync<ArgumentNullException>(() => breaker.RaiseExceptionAsync<ArgumentNullException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<ArgumentNullException>(() => breaker.RaiseExceptionAsync<ArgumentNullException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<ArgumentNullException>(() => breaker.RaiseExceptionAsync<ArgumentNullException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_exception_raised_is_not_one_of_the_specified_exceptions()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .Or<ArgumentOutOfRangeException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await Should.ThrowAsync<ArgumentNullException>(() => breaker.RaiseExceptionAsync<ArgumentNullException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<ArgumentNullException>(() => breaker.RaiseExceptionAsync<ArgumentNullException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<ArgumentNullException>(() => breaker.RaiseExceptionAsync<ArgumentNullException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    #endregion

    #region Circuit-breaker open->half-open->open/closed tests

    [Fact]
    public async Task Should_halfopen_circuit_after_the_specified_duration_has_passed()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration raises an exception, so circuit should break again
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_raise_an_exception()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // circuit has been reset so should once again allow 2 exceptions to be raised before breaking
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__test_execution_permit_directly()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        // exception raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should permit first execution.
        Should.NotThrow(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should reject a second execution.
        Should.Throw<BrokenCircuitException>(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_allow_single_execution_per_break_duration_in_halfopen_state__test_execution_permit_directly()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        // exception raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should permit first execution.
        Should.NotThrow(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should reject a second execution.
        Should.Throw<BrokenCircuitException>(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // Allow another time window to pass (breaker should still be HalfOpen).
        SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should now permit another trial execution.
        Should.NotThrow(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__integration_test()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        // exception raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // Start one execution during the HalfOpen state, and request a second execution before the first has completed (ie still during the HalfOpen state).
        // The second execution should be rejected due to the halfopen state.

        TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
        using ManualResetEvent permitSecondExecutionAttempt = new ManualResetEvent(false);
        using ManualResetEvent permitFirstExecutionEnd = new ManualResetEvent(false);
        bool? firstDelegateExecutedInHalfOpenState = null;
        bool? secondDelegateExecutedInHalfOpenState = null;
        bool? secondDelegateRejectedInHalfOpenState = null;

        bool firstExecutionActive = false;

        // First execution in HalfOpen state: we should be able to verify state is HalfOpen as it executes.
        Task firstExecution = Task.Factory.StartNew(async () =>
        {
            await Should.NotThrowAsync(() => breaker.ExecuteAsync(async () =>
            {
                firstDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                // Signal the second execution can start, overlapping with this (the first) execution.
                firstExecutionActive = true;
                permitSecondExecutionAttempt.Set();

                // Hold first execution open until second indicates it is no longer needed, or time out.
                permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                await TaskHelper.EmptyTask;
                firstExecutionActive = false;
            }));
        }, TaskCreationOptions.LongRunning);

        // Attempt a second execution, signalled by the first execution to ensure they overlap: we should be able to verify it doesn't execute, and is rejected by a breaker in a HalfOpen state.
        permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks);

        Task secondExecution = Task.Factory.StartNew(async () =>
        {
            // Validation of correct sequencing and overlapping of tasks in test (guard against erroneous test refactorings/operation).
            firstExecutionActive.ShouldBeTrue();
            breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

            try
            {
                await breaker.ExecuteAsync(async () =>
                {
                    secondDelegateRejectedInHalfOpenState = false;
                    secondDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.
                    await TaskHelper.EmptyTask;
                });
            }
            catch (BrokenCircuitException)
            {
                secondDelegateExecutedInHalfOpenState = false;
                secondDelegateRejectedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested here.
            }

            // Release first execution soon as second overlapping execution is done gathering data.
            permitFirstExecutionEnd.Set();
        }, TaskCreationOptions.LongRunning);

        // Graceful cleanup: allow executions time to end naturally; signal them to end if not; timeout any deadlocks; expose any execution faults. This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
        permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
        permitFirstExecutionEnd.Set();

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        Task.WaitAll([firstExecution, secondExecution], testTimeoutToExposeDeadlocks).ShouldBeTrue();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        if (firstExecution.IsFaulted)
        {
            throw firstExecution!.Exception!;
        }

        if (secondExecution.IsFaulted)
        {
            throw secondExecution!.Exception!;
        }

        firstExecution.Status.ShouldBe(TaskStatus.RanToCompletion);
        secondExecution.Status.ShouldBe(TaskStatus.RanToCompletion);

        // Assert:
        // - First execution should have been permitted and executed under a HalfOpen state
        // - Second overlapping execution in halfopen state should not have been permitted.
        // - Second execution attempt should have been rejected with HalfOpen state as cause.
        firstDelegateExecutedInHalfOpenState.ShouldNotBeNull();
        firstDelegateExecutedInHalfOpenState.Value.ShouldBeTrue();
        secondDelegateExecutedInHalfOpenState.ShouldNotBeNull();
        secondDelegateExecutedInHalfOpenState.Value.ShouldBeFalse();
        secondDelegateRejectedInHalfOpenState.ShouldNotBeNull();
        secondDelegateRejectedInHalfOpenState.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_allow_single_execution_per_break_duration_in_halfopen_state__integration_test()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        // exception raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // Start one execution during the HalfOpen state.
        // Request a second execution while the first is still in flight (not completed), while still during the HalfOpen state, but after one breakDuration later.
        // The second execution should be accepted in the halfopen state due to being requested after one breakDuration later.

        TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
        using ManualResetEvent permitSecondExecutionAttempt = new ManualResetEvent(false);
        using ManualResetEvent permitFirstExecutionEnd = new ManualResetEvent(false);
        bool? firstDelegateExecutedInHalfOpenState = null;
        bool? secondDelegateExecutedInHalfOpenState = null;
        bool? secondDelegateRejectedInHalfOpenState = null;

        bool firstExecutionActive = false;

        // First execution in HalfOpen state: we should be able to verify state is HalfOpen as it executes.
        Task firstExecution = Task.Factory.StartNew(async () =>
        {
            await Should.NotThrowAsync(() => breaker.ExecuteAsync(async () =>
            {
                firstDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                // Signal the second execution can start, overlapping with this (the first) execution.
                firstExecutionActive = true;
                permitSecondExecutionAttempt.Set();

                // Hold first execution open until second indicates it is no longer needed, or time out.
                permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                await TaskHelper.EmptyTask;
                firstExecutionActive = false;
            }));
        }, TaskCreationOptions.LongRunning);

        // Attempt a second execution, signalled by the first execution to ensure they overlap; start it one breakDuration later.  We should be able to verify it does execute, though the breaker is still in a HalfOpen state.
        permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks);

        Task secondExecution = Task.Factory.StartNew(async () =>
        {
            // Validation of correct sequencing and overlapping of tasks in test (guard against erroneous test refactorings/operation).
            firstExecutionActive.ShouldBeTrue();
            breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

            try
            {
                SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);

                await breaker.ExecuteAsync(async () =>
                {
                    secondDelegateRejectedInHalfOpenState = false;
                    secondDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                    await TaskHelper.EmptyTask;
                });
            }
            catch (BrokenCircuitException)
            {
                secondDelegateExecutedInHalfOpenState = false;
                secondDelegateRejectedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested here.
            }

            // Release first execution soon as second overlapping execution is done gathering data.
            permitFirstExecutionEnd.Set();
        }, TaskCreationOptions.LongRunning);

        // Graceful cleanup: allow executions time to end naturally; signal them to end if not; timeout any deadlocks; expose any execution faults. This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
        permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
        permitFirstExecutionEnd.Set();

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        Task.WaitAll([firstExecution, secondExecution], testTimeoutToExposeDeadlocks).ShouldBeTrue();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        if (firstExecution.IsFaulted)
        {
            throw firstExecution!.Exception!;
        }

        if (secondExecution.IsFaulted)
        {
            throw secondExecution!.Exception!;
        }

        firstExecution.Status.ShouldBe(TaskStatus.RanToCompletion);
        secondExecution.Status.ShouldBe(TaskStatus.RanToCompletion);

        // Assert:
        // - First execution should have been permitted and executed under a HalfOpen state
        // - Second overlapping execution in halfopen state should have been permitted, one breakDuration later.
        firstDelegateExecutedInHalfOpenState.ShouldNotBeNull();
        firstDelegateExecutedInHalfOpenState.Value.ShouldBeTrue();
        secondDelegateExecutedInHalfOpenState.ShouldNotBeNull();
        secondDelegateExecutedInHalfOpenState.Value.ShouldBeTrue();
        secondDelegateRejectedInHalfOpenState.ShouldNotBeNull();
        secondDelegateRejectedInHalfOpenState.Value.ShouldBeFalse();
    }

    #endregion

    #region Isolate and reset tests

    [Fact]
    public async Task Should_open_circuit_and_block_calls_if_manual_override_open()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // manually break circuit
        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        // circuit manually broken: execution should be blocked; even non-exception-throwing executions should not reset circuit
        bool delegateExecutedWhenBroken = false;
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }));
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);
        breaker.LastException.ShouldBeOfType<IsolatedCircuitException>();
        delegateExecutedWhenBroken.ShouldBeFalse();

    }

    [Fact]
    public async Task Should_hold_circuit_open_despite_elapsed_time_if_manual_override_open()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);
        bool delegateExecutedWhenBroken = false;
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }));
        delegateExecutedWhenBroken.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_close_circuit_again_on_reset_after_manual_override()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));

        breaker.Reset();
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
    }

    [Fact]
    public async Task Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // reset circuit, with no time having passed
        breaker.Reset();
        SystemClock.UtcNow().ShouldBe(time);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
    }

    #endregion

    #region State-change delegate tests

    [Fact]
    public void Should_not_call_onreset_on_initialise()
    {
        Action<Exception, TimeSpan> onBreak = (_, _) => { };
        bool onResetCalled = false;
        Action onReset = () => { onResetCalled = true; };

        Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        onResetCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_automatically()
    {
        bool onBreakCalled = false;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled = true; };
        Action onReset = () => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBeFalse();

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBeTrue();
    }

    [Fact]
    public void Should_call_onbreak_when_breaking_circuit_manually()
    {
        bool onBreakCalled = false;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled = true; };
        Action onReset = () => { };

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);
        onBreakCalled.ShouldBeFalse();

        breaker.Isolate();

        onBreakCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_placed_through_open_circuit()
    {
        int onBreakCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        // call through circuit when already broken - should not retrigger onBreak
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_call_failure_which_arrives_on_open_state_though_started_on_closed_state()
    {
        int onBreakCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1), onBreak, onReset);

        // Start an execution when the breaker is in the closed state, but hold it from returning (its failure) until the breaker has opened.  This call, a failure hitting an already open breaker, should indicate its fail, but should not cause onBreak() to be called a second time.
        TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
        using ManualResetEvent permitLongRunningExecutionToReturnItsFailure = new ManualResetEvent(false);
        using ManualResetEvent permitMainThreadToOpenCircuit = new ManualResetEvent(false);
        Task longRunningExecution = Task.Factory.StartNew(async () =>
        {
            breaker.CircuitState.ShouldBe(CircuitState.Closed);

            // However, since execution started when circuit was closed, BrokenCircuitException will not have been thrown on entry; the original exception will still be thrown.
            await Should.ThrowAsync<DivideByZeroException>(() => breaker.ExecuteAsync(async () =>
            {
                await TaskHelper.EmptyTask;

                permitMainThreadToOpenCircuit.Set();

                // Hold this execution until rest of the test indicates it can proceed (or timeout, to expose deadlocks).
                permitLongRunningExecutionToReturnItsFailure.WaitOne(testTimeoutToExposeDeadlocks);

                // Throw a further failure when rest of test has already broken the circuit.
                breaker.CircuitState.ShouldBe(CircuitState.Open);
                throw new DivideByZeroException();

            }));
        }, TaskCreationOptions.LongRunning);

        permitMainThreadToOpenCircuit.WaitOne(testTimeoutToExposeDeadlocks).ShouldBeTrue();

        // Break circuit in the normal manner: onBreak() should be called once.
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        // Permit the second (long-running) execution to hit the open circuit with its failure.
        permitLongRunningExecutionToReturnItsFailure.Set();

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        // Graceful cleanup: allow executions time to end naturally; timeout if any deadlocks; expose any execution faults.  This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
#if NET
        longRunningExecution.Wait(testTimeoutToExposeDeadlocks, CancellationToken.None).ShouldBeTrue();
#else
        longRunningExecution.Wait(testTimeoutToExposeDeadlocks).ShouldBeTrue();
#endif
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        if (longRunningExecution.IsFaulted)
        {
            throw longRunningExecution!.Exception!;
        }

        longRunningExecution.Status.ShouldBe(TaskStatus.RanToCompletion);

        // onBreak() should still only have been called once.
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);
    }

    [Fact]
    public async Task Should_call_onreset_when_automatically_closing_circuit_but_not_when_halfopen()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        onBreakCalled.ShouldBe(1);

        // 2 exception raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // but not yet reset
        onResetCalled.ShouldBe(0);

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBe(1);
    }

    [Fact]
    public async Task Should_not_call_onreset_on_successive_successful_calls()
    {
        Action<Exception, TimeSpan> onBreak = (_, _) => { };
        bool onResetCalled = false;
        Action onReset = () => { onResetCalled = true; };

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        onResetCalled.ShouldBeFalse();

        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBeFalse();

        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_subsequent_execution()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        int onHalfOpenCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;
        Action onHalfOpen = () => { onHalfOpenCalled++; };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        onBreakCalled.ShouldBe(1);

        // 2 exception raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        onHalfOpenCalled.ShouldBe(0); // not yet transitioned to half-open, because we have not queried state

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        onHalfOpenCalled.ShouldBe(1);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBe(1);
    }

    [Fact]
    public async Task Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_state_read()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        int onHalfOpenCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;
        Action onHalfOpen = () => onHalfOpenCalled++;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        onBreakCalled.ShouldBe(1);

        // 2 exception raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
        onHalfOpenCalled.ShouldBe(1);
        onResetCalled.ShouldBe(0);
    }

    [Fact]
    public async Task Should_call_onreset_when_manually_resetting_circuit()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        onBreakCalled.ShouldBe(0);
        breaker.Isolate();
        onBreakCalled.ShouldBe(1);

        breaker.CircuitState.ShouldBe(CircuitState.Isolated);
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));

        onResetCalled.ShouldBe(0);
        breaker.Reset();
        onResetCalled.ShouldBe(1);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
    }

    #region Tests of supplied parameters to onBreak delegate

    [Fact]
    public async Task Should_call_onbreak_with_the_last_raised_exception()
    {
        Exception? passedException = null;

        Action<Exception, TimeSpan, Context> onBreak = (exception, _, _) => passedException = exception;
        Action<Context> onReset = _ => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        passedException?.ShouldBeOfType<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_call_onbreak_with_a_state_of_closed()
    {
        CircuitState? transitionedState = null;

        Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, _, _) => transitionedState = state;
        Action<Context> onReset = _ => { };
        Action onHalfOpen = () => { };

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset, onHalfOpen);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        transitionedState?.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_call_onbreak_with_a_state_of_half_open()
    {
        List<CircuitState> transitionedStates = [];

        Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, _, _) => transitionedStates.Add(state);
        Action<Context> onReset = _ => { };
        Action onHalfOpen = () => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset, onHalfOpen);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration raises an exception, so circuit should break again
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        transitionedStates[0].ShouldBe(CircuitState.Closed);
        transitionedStates[1].ShouldBe(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_rethrow_and_call_onbreak_with_the_last_raised_exception_unwrapped_if_matched_as_inner()
    {
        Exception? passedException = null;

        Action<Exception, TimeSpan, Context> onBreak = (exception, _, _) => passedException = exception;
        Action<Context> onReset = _ => { };

        TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .HandleInner<DivideByZeroException>()
            .Or<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        Exception toRaiseAsInner = new DivideByZeroException();
        Exception withInner = new AggregateException(toRaiseAsInner);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        var ex = await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync(withInner));
        ex.ShouldBeSameAs(toRaiseAsInner);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        passedException?.ShouldBeSameAs(toRaiseAsInner);
    }

    [Fact]
    public async Task Should_call_onbreak_with_the_correct_timespan()
    {
        TimeSpan? passedBreakTimespan = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { passedBreakTimespan = timespan; };
        Action<Context> onReset = _ => { };

        TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        passedBreakTimespan.ShouldBe(durationOfBreak);
    }

    [Fact]
    public void Should_open_circuit_with_timespan_maxvalue_if_manual_override_open()
    {
        TimeSpan? passedBreakTimespan = null;
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => passedBreakTimespan = timespan;
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // manually break circuit
        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        passedBreakTimespan.ShouldBe(TimeSpan.MaxValue);
    }

    #endregion

    #region Tests that supplied context is passed to stage-change delegates

    [Fact]
    public async Task Should_call_onbreak_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) => contextData = context;
        Action<Context> onReset = _ => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        await Should.ThrowAsync<DivideByZeroException>(
            () => breaker.RaiseExceptionAsync<DivideByZeroException>(CreateDictionary("key1", "value1", "key2", "value2")));

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_onreset_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, _, _) => { };
        Action<Context> onReset = context => contextData = context;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration should invoke onReset, with context
        await breaker.ExecuteAsync(_ => TaskHelper.EmptyTask, CreateDictionary("key1", "value1", "key2", "value2"));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        IDictionary<string, object> contextData = CreateDictionary("key1", "value1", "key2", "value2");

        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) => contextData = context;
        Action<Context> onReset = _ => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        contextData.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };
        Action<Context> onReset = context => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        // 2 exception raised, circuit is now open
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>(CreateDictionary("key", "original_value")));
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        contextValue.ShouldBe("original_value");

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // but not yet reset

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(_ => TaskHelper.EmptyTask, CreateDictionary("key", "new_value"));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        contextValue.ShouldBe("new_value");
    }

    #endregion

    #endregion

    #region LastException property

    [Fact]
    public void Should_initialise_LastException_to_null_on_creation()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        breaker.LastException.ShouldBeNull();
    }

    [Fact]
    public async Task Should_set_LastException_on_handling_exception_even_when_not_breaking()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.LastException.ShouldBeOfType<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_set_LastException_on_handling_inner_exception_even_when_not_breaking()
    {
        var breaker = Policy
            .HandleInner<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        Exception toRaiseAsInner = new DivideByZeroException();
        Exception withInner = new AggregateException(toRaiseAsInner);

        var ex = await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync(withInner));
        ex.ShouldBeSameAs(toRaiseAsInner);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.LastException.ShouldBeSameAs(toRaiseAsInner);
    }

    [Fact]
    public async Task Should_set_LastException_to_last_raised_exception_when_breaking()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        breaker.LastException.ShouldBeOfType<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_set_LastException_to_null_on_circuit_reset()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        breaker.LastException.ShouldBeOfType<DivideByZeroException>();

        breaker.Reset();

        breaker.LastException.ShouldBeNull();
    }

    #endregion

    #region Cancellation support

    [Fact]
    public async Task Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            await Should.NotThrowAsync(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);
    }

    [Fact]
    public async Task Should_report_cancellation_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_action_execution_when_user_delegate_observes_cancellationToken()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_faulting_from_faulting_action_execution_when_user_delegate_does_not_observe_cancellation()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_when_both_open_circuit_and_cancellation()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();

        // Circuit is now broken.

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = null, // Cancelled manually instead - see above.
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var ex2 = await Should.ThrowAsync<OperationCanceledException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex2.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);
    }

    [Fact]
    public async Task Should_honour_different_cancellationToken_captured_implicitly_by_action()
    {
        // Before CancellationToken support was built in to Polly, users of the library may have implicitly captured a CancellationToken and used it to cancel actions.  For backwards compatibility, Polly should not confuse these with its own CancellationToken; it should distinguish TaskCanceledExceptions thrown with different CancellationTokens.

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;

        using (var policyCancellationTokenSource = new CancellationTokenSource())
        using (var implicitlyCapturedActionCancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken policyCancellationToken = policyCancellationTokenSource.Token;
            CancellationToken implicitlyCapturedActionCancellationToken = implicitlyCapturedActionCancellationTokenSource.Token;

            implicitlyCapturedActionCancellationTokenSource.Cancel();

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => breaker.ExecuteAsync(async _ =>
            {
                attemptsInvoked++;
                await TaskHelper.EmptyTask;
                implicitlyCapturedActionCancellationToken.ThrowIfCancellationRequested();
            }, policyCancellationToken));
            ex.CancellationToken.ShouldBe(implicitlyCapturedActionCancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_execute_func_returning_value_when_cancellationToken_not_cancelled()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Func<Task> action = async () => result = await breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            await Should.NotThrowAsync(action);
        }

        result.ShouldNotBeNull();
        result.Value.ShouldBeTrue();

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_honour_and_report_cancellation_during_func_execution()
    {
        var breaker = Policy
                         .Handle<DivideByZeroException>()
                         .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Func<Task> action = async () => result = await breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            var ex = await Should.ThrowAsync<OperationCanceledException>(action);
            ex.CancellationToken.ShouldBe(cancellationToken);

            result.ShouldBeNull();
        }

        attemptsInvoked.ShouldBe(1);
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
