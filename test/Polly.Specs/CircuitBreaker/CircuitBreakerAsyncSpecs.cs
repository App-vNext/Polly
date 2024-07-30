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

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
    }

    [Fact]
    public void Should_throw_if_exceptions_allowed_before_breaking_is_less_than_one()
    {
        Action action = () => Policy
                                 .Handle<DivideByZeroException>()
                                 .CircuitBreakerAsync(0, TimeSpan.FromSeconds(10));

        action.Should().Throw<ArgumentOutOfRangeException>()
              .And.ParamName.Should()
              .Be("exceptionsAllowedBeforeBreaking");
    }

    [Fact]
    public void Should_throw_if_duration_of_break_is_less_than_zero()
    {
        Action action = () => Policy
                                 .Handle<DivideByZeroException>()
                                 .CircuitBreakerAsync(1, -TimeSpan.FromSeconds(1));

        action.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should()
            .Be("durationOfBreak");
    }

    [Fact]
    public void Should_be_able_to_handle_a_duration_of_break_of_zero()
    {
        Action action = () => Policy
                                 .Handle<DivideByZeroException>()
                                 .CircuitBreakerAsync(1, TimeSpan.Zero);
        action.Should().NotThrow();
    }

    [Fact]
    public void Should_initialise_to_closed_state()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak);

        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    #endregion

    #region Circuit-breaker threshold-to-break tests

    [Fact]
    public async Task Should_not_open_circuit_if_specified_number_of_specified_exception_are_not_raised_consecutively()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(b => b.ExecuteAsync(() => TaskHelper.EmptyTask)).Should().NotThrowAsync();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_after_specified_number_of_specified_exception_have_been_raised()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        bool delegateExecutedWhenBroken = false;
        var ex = await breaker.Awaiting(x => x.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }))
              .Should().ThrowAsync<BrokenCircuitException>()
              .WithMessage("The circuit is now open and is not allowing calls.");
        ex.WithInnerException<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        delegateExecutedWhenBroken.Should().BeFalse();

    }

    [Fact]
    public async Task Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_after_specified_number_of_one_of_the_specified_exceptions_have_been_raised()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .Or<ArgumentOutOfRangeException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentOutOfRangeException>())
              .Should().ThrowAsync<ArgumentOutOfRangeException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // 2 exception raised, circuit is now open
        bool delegateExecutedWhenBroken = false;
        var ex = await breaker.Awaiting(x => x.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }))
              .Should().ThrowAsync<BrokenCircuitException>()
              .WithMessage("The circuit is now open and is not allowing calls.");
        ex.WithInnerException<ArgumentOutOfRangeException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        delegateExecutedWhenBroken.Should().BeFalse();
    }

    [Fact]
    public async Task Should_not_open_circuit_if_exception_raised_is_not_the_specified_exception()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
              .Should().ThrowAsync<ArgumentNullException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
              .Should().ThrowAsync<ArgumentNullException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
              .Should().ThrowAsync<ArgumentNullException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_exception_raised_is_not_one_of_the_specified_exceptions()
    {
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .Or<ArgumentOutOfRangeException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
              .Should().ThrowAsync<ArgumentNullException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
              .Should().ThrowAsync<ArgumentNullException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
              .Should().ThrowAsync<ArgumentNullException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    #endregion

    #region Circuit-breaker open->half-open->open/closed tests

    [Fact]
    public async Task Should_halfopen_circuit_after_the_specified_duration_has_passed()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // first call after duration raises an exception, so circuit should break again
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
    }

    [Fact]
    public async Task Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_raise_an_exception()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // circuit has been reset so should once again allow 2 exceptions to be raised before breaking
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
    }

    [Fact]
    public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__test_execution_permit_directly()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, durationOfBreak);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        // exception raised, circuit is now open.
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // OnActionPreExecute() should permit first execution.
        breaker.BreakerController.Invoking(c => c.OnActionPreExecute()).Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // OnActionPreExecute() should reject a second execution.
        breaker.BreakerController.Invoking(c => c.OnActionPreExecute()).Should().Throw<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_allow_single_execution_per_break_duration_in_halfopen_state__test_execution_permit_directly()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, durationOfBreak);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        // exception raised, circuit is now open.
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // OnActionPreExecute() should permit first execution.
        breaker.BreakerController.Invoking(c => c.OnActionPreExecute()).Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // OnActionPreExecute() should reject a second execution.
        breaker.BreakerController.Invoking(c => c.OnActionPreExecute()).Should().Throw<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // Allow another time window to pass (breaker should still be HalfOpen).
        SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // OnActionPreExecute() should now permit another trial execution.
        breaker.BreakerController.Invoking(c => c.OnActionPreExecute()).Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__integration_test()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, durationOfBreak);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        // exception raised, circuit is now open.
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

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
            await breaker.Awaiting(x => x.ExecuteAsync(async () =>
            {
                firstDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                // Signal the second execution can start, overlapping with this (the first) execution.
                firstExecutionActive = true;
                permitSecondExecutionAttempt.Set();

                // Hold first execution open until second indicates it is no longer needed, or time out.
                permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                await TaskHelper.EmptyTask;
                firstExecutionActive = false;

            })).Should().NotThrowAsync();
        }, TaskCreationOptions.LongRunning);

        // Attempt a second execution, signalled by the first execution to ensure they overlap: we should be able to verify it doesn't execute, and is rejected by a breaker in a HalfOpen state.
        permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks);

        Task secondExecution = Task.Factory.StartNew(async () =>
        {
            // Validation of correct sequencing and overlapping of tasks in test (guard against erroneous test refactorings/operation).
            firstExecutionActive.Should().BeTrue();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

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
        Task.WaitAll(new[] { firstExecution, secondExecution }, testTimeoutToExposeDeadlocks).Should().BeTrue();
        if (firstExecution.IsFaulted)
        {
            throw firstExecution!.Exception!;
        }

        if (secondExecution.IsFaulted)
        {
            throw secondExecution!.Exception!;
        }

        firstExecution.Status.Should().Be(TaskStatus.RanToCompletion);
        secondExecution.Status.Should().Be(TaskStatus.RanToCompletion);

        // Assert:
        // - First execution should have been permitted and executed under a HalfOpen state
        // - Second overlapping execution in halfopen state should not have been permitted.
        // - Second execution attempt should have been rejected with HalfOpen state as cause.
        firstDelegateExecutedInHalfOpenState.Should().BeTrue();
        secondDelegateExecutedInHalfOpenState.Should().BeFalse();
        secondDelegateRejectedInHalfOpenState.Should().BeTrue();
    }

    [Fact]
    public async Task Should_allow_single_execution_per_break_duration_in_halfopen_state__integration_test()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(1, durationOfBreak);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        // exception raised, circuit is now open.
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

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
            await breaker.Awaiting(x => x.ExecuteAsync(async () =>
            {
                firstDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                // Signal the second execution can start, overlapping with this (the first) execution.
                firstExecutionActive = true;
                permitSecondExecutionAttempt.Set();

                // Hold first execution open until second indicates it is no longer needed, or time out.
                permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                await TaskHelper.EmptyTask;
                firstExecutionActive = false;
            })).Should().NotThrowAsync();
        }, TaskCreationOptions.LongRunning);

        // Attempt a second execution, signalled by the first execution to ensure they overlap; start it one breakDuration later.  We should be able to verify it does execute, though the breaker is still in a HalfOpen state.
        permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks);

        Task secondExecution = Task.Factory.StartNew(async () =>
        {
            // Validation of correct sequencing and overlapping of tasks in test (guard against erroneous test refactorings/operation).
            firstExecutionActive.Should().BeTrue();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

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
        Task.WaitAll(new[] { firstExecution, secondExecution }, testTimeoutToExposeDeadlocks).Should().BeTrue();
        if (firstExecution.IsFaulted)
        {
            throw firstExecution!.Exception!;
        }

        if (secondExecution.IsFaulted)
        {
            throw secondExecution!.Exception!;
        }

        firstExecution.Status.Should().Be(TaskStatus.RanToCompletion);
        secondExecution.Status.Should().Be(TaskStatus.RanToCompletion);

        // Assert:
        // - First execution should have been permitted and executed under a HalfOpen state
        // - Second overlapping execution in halfopen state should have been permitted, one breakDuration later.
        firstDelegateExecutedInHalfOpenState.Should().BeTrue();
        secondDelegateExecutedInHalfOpenState.Should().BeTrue();
        secondDelegateRejectedInHalfOpenState.Should().BeFalse();
    }

    #endregion

    #region Isolate and reset tests

    [Fact]
    public async Task Should_open_circuit_and_block_calls_if_manual_override_open()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // manually break circuit
        breaker.Isolate();
        breaker.CircuitState.Should().Be(CircuitState.Isolated);

        // circuit manually broken: execution should be blocked; even non-exception-throwing executions should not reset circuit
        bool delegateExecutedWhenBroken = false;
        await breaker.Awaiting(x => x.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }))
            .Should().ThrowAsync<IsolatedCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Isolated);
        breaker.LastException.Should().BeOfType<IsolatedCircuitException>();
        delegateExecutedWhenBroken.Should().BeFalse();

    }

    [Fact]
    public async Task Should_hold_circuit_open_despite_elapsed_time_if_manual_override_open()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Isolate();
        breaker.CircuitState.Should().Be(CircuitState.Isolated);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.Isolated);
        bool delegateExecutedWhenBroken = false;
        await breaker.Awaiting(x => x.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }))
            .Should().ThrowAsync<IsolatedCircuitException>();
        delegateExecutedWhenBroken.Should().BeFalse();
    }

    [Fact]
    public async Task Should_close_circuit_again_on_reset_after_manual_override()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Isolate();
        breaker.CircuitState.Should().Be(CircuitState.Isolated);
        await breaker.Awaiting(x => x.ExecuteAsync(() => TaskHelper.EmptyTask))
            .Should().ThrowAsync<IsolatedCircuitException>();

        breaker.Reset();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        await breaker.Awaiting(x => x.ExecuteAsync(() => TaskHelper.EmptyTask)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // reset circuit, with no time having passed
        breaker.Reset();
        SystemClock.UtcNow().Should().Be(time);
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        await breaker.Awaiting(x => x.ExecuteAsync(() => TaskHelper.EmptyTask)).Should().NotThrowAsync();
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

        onResetCalled.Should().BeFalse();
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

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().BeFalse();

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().BeTrue();
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
        onBreakCalled.Should().BeFalse();

        breaker.Isolate();

        onBreakCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_placed_through_open_circuit()
    {
        int onBreakCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().Be(1);

        // call through circuit when already broken - should not retrigger onBreak
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().Be(1);
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
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            await breaker.Awaiting(x => x.ExecuteAsync(async () =>
            {
                await TaskHelper.EmptyTask;

                permitMainThreadToOpenCircuit.Set();

                // Hold this execution until rest of the test indicates it can proceed (or timeout, to expose deadlocks).
                permitLongRunningExecutionToReturnItsFailure.WaitOne(testTimeoutToExposeDeadlocks);

                // Throw a further failure when rest of test has already broken the circuit.
                breaker.CircuitState.Should().Be(CircuitState.Open);
                throw new DivideByZeroException();

            })).Should().ThrowAsync<DivideByZeroException>(); // However, since execution started when circuit was closed, BrokenCircuitException will not have been thrown on entry; the original exception will still be thrown.
        }, TaskCreationOptions.LongRunning);

        permitMainThreadToOpenCircuit.WaitOne(testTimeoutToExposeDeadlocks).Should().BeTrue();

        // Break circuit in the normal manner: onBreak() should be called once.
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().Be(1);

        // Permit the second (long-running) execution to hit the open circuit with its failure.
        permitLongRunningExecutionToReturnItsFailure.Set();

        // Graceful cleanup: allow executions time to end naturally; timeout if any deadlocks; expose any execution faults.  This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
        longRunningExecution.Wait(testTimeoutToExposeDeadlocks).Should().BeTrue();
        if (longRunningExecution.IsFaulted)
        {
            throw longRunningExecution!.Exception!;
        }

        longRunningExecution.Status.Should().Be(TaskStatus.RanToCompletion);

        // onBreak() should still only have been called once.
        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().Be(1);
    }

    [Fact]
    public async Task Should_call_onreset_when_automatically_closing_circuit_but_not_when_halfopen()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { onResetCalled++; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        onBreakCalled.Should().Be(0);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        onBreakCalled.Should().Be(0);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        onBreakCalled.Should().Be(1);

        // 2 exception raised, circuit is now open
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().Be(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // but not yet reset
        onResetCalled.Should().Be(0);

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onResetCalled.Should().Be(1);
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

        onResetCalled.Should().BeFalse();

        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onResetCalled.Should().BeFalse();

        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onResetCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_subsequent_execution()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        int onHalfOpenCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { onResetCalled++; };
        Action onHalfOpen = () => { onHalfOpenCalled++; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

        onBreakCalled.Should().Be(0);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        onBreakCalled.Should().Be(0);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        onBreakCalled.Should().Be(1);

        // 2 exception raised, circuit is now open
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().Be(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        onHalfOpenCalled.Should().Be(0); // not yet transitioned to half-open, because we have not queried state

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        onHalfOpenCalled.Should().Be(1);
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onResetCalled.Should().Be(1);
    }

    [Fact]
    public async Task Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_state_read()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        int onHalfOpenCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { onResetCalled++; };
        Action onHalfOpen = () => { onHalfOpenCalled++; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

        onBreakCalled.Should().Be(0);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        onBreakCalled.Should().Be(0);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        onBreakCalled.Should().Be(1);

        // 2 exception raised, circuit is now open
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().Be(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
        onHalfOpenCalled.Should().Be(1);
        onResetCalled.Should().Be(0);
    }

    [Fact]
    public async Task Should_call_onreset_when_manually_resetting_circuit()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { onResetCalled++; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        onBreakCalled.Should().Be(0);
        breaker.Isolate();
        onBreakCalled.Should().Be(1);

        breaker.CircuitState.Should().Be(CircuitState.Isolated);
        await breaker.Awaiting(x => x.ExecuteAsync(() => TaskHelper.EmptyTask))
            .Should().ThrowAsync<IsolatedCircuitException>();

        onResetCalled.Should().Be(0);
        breaker.Reset();
        onResetCalled.Should().Be(1);

        breaker.CircuitState.Should().Be(CircuitState.Closed);
        await breaker.Awaiting(x => x.ExecuteAsync(() => TaskHelper.EmptyTask)).Should().NotThrowAsync();
    }

    #region Tests of supplied parameters to onBreak delegate

    [Fact]
    public async Task Should_call_onbreak_with_the_last_raised_exception()
    {
        Exception? passedException = null;

        Action<Exception, TimeSpan, Context> onBreak = (exception, _, _) => { passedException = exception; };
        Action<Context> onReset = _ => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        passedException?.Should().BeOfType<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_call_onbreak_with_a_state_of_closed()
    {
        CircuitState? transitionedState = null;

        Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, _, _) => { transitionedState = state; };
        Action<Context> onReset = _ => { };
        Action onHalfOpen = () => { };

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset, onHalfOpen);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        transitionedState?.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_call_onbreak_with_a_state_of_half_open()
    {
        List<CircuitState> transitionedStates = [];

        Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, _, _) => { transitionedStates.Add(state); };
        Action<Context> onReset = _ => { };
        Action onHalfOpen = () => { };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset, onHalfOpen);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // 2 exception raised, circuit is now open
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // first call after duration raises an exception, so circuit should break again
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<BrokenCircuitException>();

        transitionedStates[0].Should().Be(CircuitState.Closed);
        transitionedStates[1].Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_rethrow_and_call_onbreak_with_the_last_raised_exception_unwrapped_if_matched_as_inner()
    {
        Exception? passedException = null;

        Action<Exception, TimeSpan, Context> onBreak = (exception, _, _) => { passedException = exception; };
        Action<Context> onReset = _ => { };

        TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .HandleInner<DivideByZeroException>()
            .Or<DivideByZeroException>()
            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        Exception toRaiseAsInner = new DivideByZeroException();
        Exception withInner = new AggregateException(toRaiseAsInner);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
            .Should().ThrowAsync<DivideByZeroException>();

        var ex = await breaker.Awaiting(x => x.RaiseExceptionAsync(withInner))
            .Should().ThrowAsync<DivideByZeroException>();
        ex.Which.Should().BeSameAs(toRaiseAsInner);

        breaker.CircuitState.Should().Be(CircuitState.Open);

        passedException?.Should().BeSameAs(toRaiseAsInner);
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

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        passedBreakTimespan.Should().Be(durationOfBreak);
    }

    [Fact]
    public void Should_open_circuit_with_timespan_maxvalue_if_manual_override_open()
    {
        TimeSpan? passedBreakTimespan = null;
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { passedBreakTimespan = timespan; };
        Action<Context> onReset = _ => { };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // manually break circuit
        breaker.Isolate();
        breaker.CircuitState.Should().Be(CircuitState.Isolated);

        passedBreakTimespan.Should().Be(TimeSpan.MaxValue);
    }

    #endregion

    #region Tests that supplied context is passed to stage-change delegates

    [Fact]
    public async Task Should_call_onbreak_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) => { contextData = context; };
        Action<Context> onReset = _ => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(
            CreateDictionary("key1", "value1", "key2", "value2"))).Should().ThrowAsync<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public async Task Should_call_onreset_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, _, _) => { };
        Action<Context> onReset = context => { contextData = context; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // first call after duration should invoke onReset, with context
        await breaker.ExecuteAsync(_ => TaskHelper.EmptyTask, CreateDictionary("key1", "value1", "key2", "value2"));

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        IDictionary<string, object> contextData = CreateDictionary("key1", "value1", "key2", "value2");

        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) => { contextData = context; };
        Action<Context> onReset = _ => { };

        var breaker = Policy
                        .Handle<DivideByZeroException>()
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        contextData.Should().BeEmpty();
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

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        // 2 exception raised, circuit is now open
        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(CreateDictionary("key", "original_value")))
              .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        contextValue.Should().Be("original_value");

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // but not yet reset

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(_ => TaskHelper.EmptyTask, CreateDictionary("key", "new_value"));
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        contextValue.Should().Be("new_value");
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

        breaker.LastException.Should().BeNull();
    }

    [Fact]
    public async Task Should_set_LastException_on_handling_exception_even_when_not_breaking()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.LastException.Should().BeOfType<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_set_LastException_on_handling_inner_exception_even_when_not_breaking()
    {
        var breaker = Policy
            .HandleInner<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        Exception toRaiseAsInner = new DivideByZeroException();
        Exception withInner = new AggregateException(toRaiseAsInner);

        var ex = await breaker.Awaiting(x => x.RaiseExceptionAsync(withInner))
            .Should().ThrowAsync<DivideByZeroException>();
        ex.Which.Should().BeSameAs(toRaiseAsInner);

        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.LastException.Should().BeSameAs(toRaiseAsInner);
    }

    [Fact]
    public async Task Should_set_LastException_to_last_raised_exception_when_breaking()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        breaker.LastException.Should().BeOfType<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_set_LastException_to_null_on_circuit_reset()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        breaker.LastException.Should().BeOfType<DivideByZeroException>();

        breaker.Reset();

        breaker.LastException.Should().BeNull();
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
            await breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().NotThrowAsync();
        }

        attemptsInvoked.Should().Be(1);
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

            var ex = await breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(0);
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

            var ex = await breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
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

            var ex = await breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
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
            await breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().ThrowAsync<DivideByZeroException>();
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public async Task Should_report_cancellation_when_both_open_circuit_and_cancellation()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1));

        await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
            .Should().ThrowAsync<DivideByZeroException>();

        var ex = await breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
            .Should().ThrowAsync<BrokenCircuitException>()
            .WithMessage("The circuit is now open and is not allowing calls.");
        ex.WithInnerException<DivideByZeroException>();

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

            var ex2 = await breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().ThrowAsync<OperationCanceledException>();
            ex2.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(0);
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

            var ex = await breaker.Awaiting(x => x.ExecuteAsync(async _ =>
            {
                attemptsInvoked++;
                await TaskHelper.EmptyTask;
                implicitlyCapturedActionCancellationToken.ThrowIfCancellationRequested();
            }, policyCancellationToken))
                .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(implicitlyCapturedActionCancellationToken);
        }

        attemptsInvoked.Should().Be(1);
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
            Func<AsyncCircuitBreakerPolicy, Task> action = async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            await breaker.Awaiting(action)
                .Should().NotThrowAsync();
        }

        result.Should().BeTrue();

        attemptsInvoked.Should().Be(1);
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

            Func<AsyncCircuitBreakerPolicy, Task> action = async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            var ex = await breaker.Awaiting(action)
                .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);

            result.Should().Be(null);
        }

        attemptsInvoked.Should().Be(1);
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
