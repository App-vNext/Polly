using Scenario = Polly.Specs.Helpers.PolicyTResultExtensionsAsync.ResultAndOrCancellationScenario;

namespace Polly.Specs.CircuitBreaker;

[Collection(Constants.SystemClockDependentTestCollection)]
public class CircuitBreakerTResultAsyncSpecs : IDisposable
{
    #region Configuration tests

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;
        PolicyBuilder<EmptyStruct> policyBuilder = new PolicyBuilder<EmptyStruct>(exception => exception);

        var exceptionsAllowedBeforeBreaking = 1;
        var durationOfBreak = TimeSpan.Zero;
        Action<DelegateResult<EmptyStruct>, CircuitState, TimeSpan, Context> onBreak = null!;
        Action<Context> onReset = null!;
        Action onHalfOpen = null!;
        ICircuitController<EmptyStruct> breakerController = new ConsecutiveCountCircuitController<EmptyStruct>(
            exceptionsAllowedBeforeBreaking,
            durationOfBreak,
            onBreak,
            onReset,
            onHalfOpen);

        var instance = Activator.CreateInstance(
            typeof(AsyncCircuitBreakerPolicy<EmptyStruct>),
            flags,
            null,
            [policyBuilder, breakerController],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public async Task Should_be_able_to_handle_a_duration_of_timespan_maxvalue()
    {
        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(1, TimeSpan.MaxValue);

        var result = await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault);
        result.ShouldBe(ResultPrimitive.Fault);
    }

    [Fact]
    public void Should_throw_if_faults_allowed_before_breaking_is_less_than_one()
    {
        Action action = () => Policy
                                 .HandleResult(ResultPrimitive.Fault)
                                 .CircuitBreakerAsync(0, TimeSpan.FromSeconds(10));

        Should.Throw<ArgumentOutOfRangeException>(action)
              .ParamName.ShouldBe("handledEventsAllowedBeforeBreaking");
    }

    [Fact]
    public void Should_throw_if_duration_of_break_is_less_than_zero()
    {
        Action action = () => Policy
                                 .HandleResult(ResultPrimitive.Fault)
                                 .CircuitBreakerAsync(1, -TimeSpan.FromSeconds(1));

        Should.Throw<ArgumentOutOfRangeException>(action)
            .ParamName.ShouldBe("durationOfBreak");
    }

    [Fact]
    public void Should_be_able_to_handle_a_duration_of_break_of_zero()
    {
        Action action = () => Policy
                                 .HandleResult(ResultPrimitive.Fault)
                                 .CircuitBreakerAsync(1, TimeSpan.Zero);
        Should.NotThrow(action);
    }

    [Fact]
    public void Should_initialise_to_closed_state()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    #endregion

    #region Circuit-breaker threshold-to-break tests

    [Fact]
    public async Task Should_not_open_circuit_if_specified_number_of_specified_handled_result_are_not_raised_consecutively()
    {
        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Good))
              .ShouldBe(ResultPrimitive.Good);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_handled_result_after_specified_number_of_specified_handled_result_have_been_returned()
    {
        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        var ex = await Should.ThrowAsync<BrokenCircuitException<ResultPrimitive>>(() => breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault));
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.Result.ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_handled_result_after_specified_number_of_one_of_the_specified_handled_results_have_been_raised()
    {
        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .OrResult(ResultPrimitive.FaultAgain)
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain))
              .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception or fault raised, circuit is now open
        var ex = await Should.ThrowAsync<BrokenCircuitException<ResultPrimitive>>(() => breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault));
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.Result.ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_handled_result_after_specified_number_of_specified_handled_result_with_predicate_have_been_returned()
    {
        var breaker = Policy
                        .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault)))
              .ResultCode.ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault)))
              .ResultCode.ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        var ex = await Should.ThrowAsync<BrokenCircuitException<ResultClass>>(() => breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Good)));
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.Result.ResultCode.ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_result_returned_is_not_the_handled_result()
    {
        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain))
              .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain))
              .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain))
              .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_result_returned_is_not_one_of_the_handled_results()
    {
        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .OrResult(ResultPrimitive.FaultYetAgain)
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain))
            .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain))
            .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain))
            .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_result_returned_does_not_match_result_predicate()
    {
        var breaker = Policy
                        .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)))
            .ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)))
            .ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)))
            .ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_result_returned_does_not_match_any_of_the_result_predicates()
    {
        var breaker = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .OrResult(r => r.ResultCode == ResultPrimitive.FaultYetAgain)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)))
            .ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)))
            .ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)))
            .ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
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
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception or fault raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
    }

    [Fact]
    public async Task Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_a_fault()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception or fault raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration returns a fault, so circuit should break again
        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault));
    }

    [Fact]
    public async Task Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_return_a_fault()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception or fault raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration is successful, so circuit should reset
        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Good))
            .ShouldBe(ResultPrimitive.Good);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // circuit has been reset so should once again allow 2 faults to be raised before breaking
        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault));
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__test_execution_permit_directly()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(1, durationOfBreak);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);

        // exception raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should permit first execution.
        Should.NotThrow(breaker.BreakerController.OnActionPreExecute);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should reject a second execution.
        Should.Throw<BrokenCircuitException>(breaker.BreakerController.OnActionPreExecute);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_allow_single_execution_per_break_duration_in_halfopen_state__test_execution_permit_directly()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(1, durationOfBreak);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);

        // exception raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should permit first execution.
        Should.NotThrow(breaker.BreakerController.OnActionPreExecute);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should reject a second execution.
        Should.Throw<BrokenCircuitException>(breaker.BreakerController.OnActionPreExecute);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // Allow another time window to pass (breaker should still be HalfOpen).
        SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should now permit another trial execution.
        Should.NotThrow(breaker.BreakerController.OnActionPreExecute);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__integration_test()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(1, durationOfBreak);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);

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
                permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks).ShouldBeTrue();
                await TaskHelper.EmptyTask;
                firstExecutionActive = false;

                return ResultPrimitive.Good;
            }));
        }, TaskCreationOptions.LongRunning);

        // Attempt a second execution, signalled by the first execution to ensure they overlap: we should be able to verify it doesn't execute, and is rejected by a breaker in a HalfOpen state.
        permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks).ShouldBeTrue();

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

                    return ResultPrimitive.Good;
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

#pragma warning disable xUnit1031
        Task.WaitAll([firstExecution, secondExecution], testTimeoutToExposeDeadlocks).ShouldBeTrue();
#pragma warning restore xUnit1031

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
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(1, durationOfBreak);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);

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

                return ResultPrimitive.Good;
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

                    return ResultPrimitive.Good;
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

#pragma warning disable xUnit1031
        Task.WaitAll([firstExecution, secondExecution], testTimeoutToExposeDeadlocks).ShouldBeTrue();
#pragma warning restore xUnit1031

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
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // manually break circuit
        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        // circuit manually broken: execution should be blocked; even non-fault-returning executions should not reset circuit
        bool delegateExecutedWhenBroken = false;
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return Task.FromResult(ResultPrimitive.Good); }));
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
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        bool delegateExecutedWhenBroken = false;
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return Task.FromResult(ResultPrimitive.Good); }));
        delegateExecutedWhenBroken.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_close_circuit_again_on_reset_after_manual_override()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));

        breaker.Reset();
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
    }

    [Fact]
    public async Task Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception or fault raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // reset circuit, with no time having passed
        breaker.Reset();
        SystemClock.UtcNow().ShouldBe(time);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
    }

    #endregion

    #region State-change delegate tests

    [Fact]
    public void Should_not_call_onreset_on_initialise()
    {
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => { };
        bool onResetCalled = false;
        Action onReset = () => onResetCalled = true;

        Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        onResetCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_automatically()
    {
        bool onBreakCalled = false;
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => onBreakCalled = true;
        Action onReset = () => { };

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBeFalse();

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBeTrue();
    }

    [Fact]
    public void Should_call_onbreak_when_breaking_circuit_manually()
    {
        bool onBreakCalled = false;
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => onBreakCalled = true;
        Action onReset = () => { };

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);
        onBreakCalled.ShouldBeFalse();

        breaker.Isolate();

        onBreakCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_placed_through_open_circuit()
    {
        int onBreakCalled = 0;
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => { };

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        // call through circuit when already broken - should not retrigger onBreak
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));

        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_call_failure_which_arrives_on_open_state_though_started_on_closed_state()
    {
        int onBreakCalled = 0;
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => { };

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1), onBreak, onReset);

        // Start an execution when the breaker is in the closed state, but hold it from returning (its failure) until the breaker has opened.  This call, a failure hitting an already open breaker, should indicate its fail, but should not cause onBreak() to be called a second time.
        TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
        using ManualResetEvent permitLongRunningExecutionToReturnItsFailure = new ManualResetEvent(false);
        using ManualResetEvent permitMainThreadToOpenCircuit = new ManualResetEvent(false);
        Task longRunningExecution = Task.Factory.StartNew(async () =>
        {
            breaker.CircuitState.ShouldBe(CircuitState.Closed);

            (await breaker.ExecuteAsync(async () =>
            {
                await TaskHelper.EmptyTask;

                permitMainThreadToOpenCircuit.Set();

                // Hold this execution until rest of the test indicates it can proceed (or timeout, to expose deadlocks).
                permitLongRunningExecutionToReturnItsFailure.WaitOne(testTimeoutToExposeDeadlocks);

                // Throw a further failure when rest of test has already broken the circuit.
                breaker.CircuitState.ShouldBe(CircuitState.Open);
                return ResultPrimitive.Fault;

            })).ShouldBe(ResultPrimitive.Fault); // However, since execution started when circuit was closed, BrokenCircuitException will not have been thrown on entry; the original fault should still be returned.
        }, TaskCreationOptions.LongRunning);

        permitMainThreadToOpenCircuit.WaitOne(testTimeoutToExposeDeadlocks).ShouldBeTrue();

        // Break circuit in the normal manner: onBreak() should be called once.
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);
        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        // Permit the second (long-running) execution to hit the open circuit with its failure.
        permitLongRunningExecutionToReturnItsFailure.Set();

#pragma warning disable xUnit1031
        // Graceful cleanup: allow executions time to end naturally; timeout if any deadlocks; expose any execution faults.  This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
#if NET
        longRunningExecution.Wait(testTimeoutToExposeDeadlocks, CancellationToken.None).ShouldBeTrue();
#else
        longRunningExecution.Wait(testTimeoutToExposeDeadlocks).ShouldBeTrue();
#endif
#pragma warning restore xUnit1031

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
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        onBreakCalled.ShouldBe(0);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        onBreakCalled.ShouldBe(0);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        onBreakCalled.ShouldBe(1);

        // 2 exception or fault raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // but not yet reset
        onResetCalled.ShouldBe(0);

        // first call after duration is successful, so circuit should reset
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBe(1);
    }

    [Fact]
    public async Task Should_not_call_onreset_on_successive_successful_calls()
    {
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => { };
        bool onResetCalled = false;
        Action onReset = () => onResetCalled = true;

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        onResetCalled.ShouldBeFalse();

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBeFalse();

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_subsequent_execution()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        int onHalfOpenCalled = 0;
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;
        Action onHalfOpen = () => onHalfOpenCalled++;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

        onBreakCalled.ShouldBe(0);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        onBreakCalled.ShouldBe(0);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        onBreakCalled.ShouldBe(1);

        // 2 exception or fault raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        onHalfOpenCalled.ShouldBe(0); // not yet transitioned to half-open, because we have not queried state

        // first call after duration is successful, so circuit should reset
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
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
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;
        Action onHalfOpen = () => onHalfOpenCalled++;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

        onBreakCalled.ShouldBe(0);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        onBreakCalled.ShouldBe(0);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        onBreakCalled.ShouldBe(1);

        // 2 exception or fault raised, circuit is now open
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
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
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        onBreakCalled.ShouldBe(0);
        breaker.Isolate();
        onBreakCalled.ShouldBe(1);

        breaker.CircuitState.ShouldBe(CircuitState.Isolated);
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));

        onResetCalled.ShouldBe(0);
        breaker.Reset();
        onResetCalled.ShouldBe(1);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)));
    }

    #region Tests of supplied parameters to onBreak delegate

    [Fact]
    public async Task Should_call_onbreak_with_the_last_handled_result()
    {
        ResultPrimitive? handledResult = null;

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (outcome, _, _) => handledResult = outcome.Result;
        Action<Context> onReset = _ => { };

        TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        handledResult?.ShouldBe(ResultPrimitive.Fault);
    }

    [Fact]
    public async Task Should_call_onbreak_with_the_correct_timespan()
    {
        TimeSpan? passedBreakTimespan = null;

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, timespan, _) => passedBreakTimespan = timespan;
        Action<Context> onReset = _ => { };

        TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        passedBreakTimespan.ShouldBe(durationOfBreak);
    }

    [Fact]
    public void Should_open_circuit_with_timespan_maxvalue_if_manual_override_open()
    {
        TimeSpan? passedBreakTimespan = null;
        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, timespan, _) => passedBreakTimespan = timespan;
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);
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

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, _, context) => contextData = context;
        Action<Context> onReset = _ => { };

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        (await breaker.RaiseResultSequenceAsync(CreateDictionary("key1", "value1", "key2", "value2"),
            ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_onreset_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, _, _) => { };
        Action<Context> onReset = context => contextData = context;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);
        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration should invoke onReset, with context
        await breaker.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Good), CreateDictionary("key1", "value1", "key2", "value2"));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        IDictionary<string, object> contextData = CreateDictionary("key1", "value1", "key2", "value2");

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, _, context) => contextData = context;
        Action<Context> onReset = _ => { };

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        contextData.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, _, context) => contextValue = context.ContainsKey("key") ? context["key"].ToString() : null;
        Action<Context> onReset = context => contextValue = context.ContainsKey("key") ? context["key"].ToString() : null;

        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        // 2 exception or fault raised, circuit is now open
        (await breaker.RaiseResultSequenceAsync(CreateDictionary("key", "original_value"), ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        contextValue.ShouldBe("original_value");

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // but not yet reset

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Good), CreateDictionary("key", "new_value"));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        contextValue.ShouldBe("new_value");
    }

    #endregion

    #endregion

    #region LastHandledResult property

    [Fact]
    public void Should_initialise_LastHandledResult_and_LastResult_to_default_on_creation()
    {
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        breaker.LastHandledResult.ShouldBe(default);
        breaker.LastException.ShouldBeNull();
    }

    [Fact]
    public async Task Should_set_LastHandledResult_on_handling_result_even_when_not_breaking()
    {
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.LastHandledResult.ShouldBe(ResultPrimitive.Fault);
        breaker.LastException.ShouldBeNull();
    }

    [Fact]
    public async Task Should_set_LastHandledResult_to_last_handled_result_when_breaking()
    {
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        breaker.LastHandledResult.ShouldBe(ResultPrimitive.Fault);
        breaker.LastException.ShouldBeNull();
    }

    [Fact]
    public async Task Should_set_LastHandledResult_to_default_on_circuit_reset()
    {
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        breaker.LastHandledResult.ShouldBe(ResultPrimitive.Fault);
        breaker.LastException.ShouldBeNull();

        breaker.Reset();

        breaker.LastHandledResult.ShouldBe(default);
        breaker.LastException.ShouldBeNull();
    }

    #endregion

    #region Cancellation support

    [Fact]
    public async Task Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            (await breaker.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                ResultPrimitive.Good))
                .ShouldBe(ResultPrimitive.Good);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => breaker.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);
    }

    [Fact]
    public async Task Should_report_cancellation_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => breaker.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Good,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_action_execution_when_user_delegate_observes_cancellationToken()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => breaker.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_faulting_from_faulting_action_execution_when_user_delegate_does_not_observe_cancellation()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .CircuitBreakerAsync(2, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            (await breaker.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault))
                            .ShouldBe(ResultPrimitive.Fault);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_when_both_open_circuit_and_cancellation()
    {
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1));

        (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault));
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");

        // Circuit is now broken.

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null, // Cancelled manually instead - see above.
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var ex2 = await Should.ThrowAsync<OperationCanceledException>(() => breaker.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good));
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
                        .HandleResult(ResultPrimitive.Fault)
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
                return ResultPrimitive.Good;
            }, policyCancellationToken));
            ex.CancellationToken.ShouldBe(implicitlyCapturedActionCancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public void Should_set_throw_if_callbacks_are_null()
    {
        var builder = Policy.HandleResult(ResultPrimitive.Fault);

        Action<DelegateResult<ResultPrimitive>, CircuitState, TimeSpan, Context> nullOnBreak = null!;
        Action<DelegateResult<ResultPrimitive>, CircuitState, TimeSpan, Context> onBreak = (_, _, _, _) => { };
        Action<Context> onReset = (_) => { };
        Action onHalfOpen = () => { };

        Should.Throw<ArgumentNullException>(() => builder.CircuitBreakerAsync(1, TimeSpan.Zero, nullOnBreak, onReset, onHalfOpen)).ParamName.ShouldBe("onBreak");
        Should.Throw<ArgumentNullException>(() => builder.CircuitBreakerAsync(1, TimeSpan.Zero, onBreak, null!, onHalfOpen)).ParamName.ShouldBe("onReset");
        Should.Throw<ArgumentNullException>(() => builder.CircuitBreakerAsync(1, TimeSpan.Zero, onBreak, onReset, null!)).ParamName.ShouldBe("onHalfOpen");
    }

    #endregion
    public void Dispose() =>
        SystemClock.Reset();
}
