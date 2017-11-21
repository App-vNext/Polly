using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

using Scenario = Polly.Specs.Helpers.PolicyTResultExtensionsAsync.ResultAndOrCancellationScenario;

namespace Polly.Specs.CircuitBreaker
{
    [Collection("SystemClockDependantCollection")]
    public class CircuitBreakerTResultAsyncSpecs : IDisposable
    {
        #region Configuration tests

        [Fact]
        public async Task Should_be_able_to_handle_a_duration_of_timespan_maxvalue()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(1, TimeSpan.MaxValue);

            var result = await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Fault);
        }

        [Fact]
        public void Should_throw_if_faults_allowed_before_breaking_is_less_than_one()
        {
            Action action = () => Policy
                                     .HandleResult(ResultPrimitive.Fault)
                                     .CircuitBreakerAsync(0, TimeSpan.FromSeconds(10));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                  .And.ParamName.Should()
                  .Be("handledEventsAllowedBeforeBreaking");
        }

        [Fact]
        public void Should_throw_if_duration_of_break_is_less_than_zero()
        {
            Action action = () => Policy
                                     .HandleResult(ResultPrimitive.Fault)
                                     .CircuitBreakerAsync(1, -TimeSpan.FromSeconds(1));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("durationOfBreak");
        }

        [Fact]
        public void Should_be_able_to_handle_a_duration_of_break_of_zero()
        {
            Action action = () => Policy
                                     .HandleResult(ResultPrimitive.Fault)
                                     .CircuitBreakerAsync(1, TimeSpan.Zero);
            action.ShouldNotThrow();
        }

        [Fact]
        public void Should_initialise_to_closed_state()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Circuit-breaker threshold-to-break tests

        [Fact]
        public async Task Should_not_open_circuit_if_specified_number_of_specified_handled_result_are_not_raised_consecutively()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Good).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Good);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public async Task Should_open_circuit_with_the_last_handled_result_after_specified_number_of_specified_handled_result_have_been_returned()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Awaiting(async b => await b.RaiseResultSequenceAsync(ResultPrimitive.Fault))
                .ShouldThrow<BrokenCircuitException<ResultPrimitive>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e.Result == ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public async Task Should_open_circuit_with_the_last_handled_result_after_specified_number_of_one_of_the_specified_handled_results_have_been_raised()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .OrResult(ResultPrimitive.FaultAgain)
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception or fault raised, circuit is now open
            breaker.Awaiting(async b => await b.RaiseResultSequenceAsync(ResultPrimitive.Fault))
                .ShouldThrow<BrokenCircuitException<ResultPrimitive>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e.Result == ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public async Task Should_open_circuit_with_the_last_handled_result_after_specified_number_of_specified_handled_result_with_predicate_have_been_returned()
        {
            CircuitBreakerPolicy<ResultClass> breaker = Policy
                            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault)).ConfigureAwait(false))
                  .ResultCode.Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault)).ConfigureAwait(false))
                  .ResultCode.Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Awaiting(async b => await b.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Good)))
                .ShouldThrow<BrokenCircuitException<ResultClass>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e.Result.ResultCode == ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public async Task Should_not_open_circuit_if_result_returned_is_not_the_handled_result()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public async Task Should_not_open_circuit_if_result_returned_is_not_one_of_the_handled_results()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .OrResult(ResultPrimitive.FaultYetAgain)
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public async Task Should_not_open_circuit_if_result_returned_does_not_match_result_predicate()
        {
            CircuitBreakerPolicy<ResultClass> breaker = Policy
                            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)).ConfigureAwait(false))
                .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)).ConfigureAwait(false))
                .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)).ConfigureAwait(false))
                .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public async Task Should_not_open_circuit_if_result_returned_does_not_match_any_of_the_result_predicates()
        {
            CircuitBreakerPolicy<ResultClass> breaker = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .OrResult(r => r.ResultCode == ResultPrimitive.FaultYetAgain)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)).ConfigureAwait(false))
                .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)).ConfigureAwait(false))
                .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain)).ConfigureAwait(false))
                .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
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

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception or fault raised, circuit is now open
            breaker.Awaiting(async b => await b.RaiseResultSequenceAsync(ResultPrimitive.Fault))
                .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
        }

        [Fact]
        public async Task Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_a_fault()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception or fault raised, circuit is now open
            breaker.Awaiting(async b => await b.RaiseResultSequenceAsync(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration returns a fault, so circuit should break again
            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);
            breaker.Awaiting(async b => await b.RaiseResultSequenceAsync(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();

        }

        [Fact]
        public async Task Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_return_a_fault()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception or fault raised, circuit is now open
            breaker.Awaiting(async b => await b.RaiseResultSequenceAsync(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // first call after duration is successful, so circuit should reset
            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Good).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Good);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // circuit has been reset so should once again allow 2 faults to be raised before breaking
            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Awaiting(async b => await b.RaiseResultSequenceAsync(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__test_execution_permit_directly()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(1, durationOfBreak);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);

            // exception raised, circuit is now open.  
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // break duration passes, circuit now half open
            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);


            // OnActionPreExecute() should permit first execution.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // OnActionPreExecute() should reject a second execution.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
        }

        [Fact]
        public async Task Should_allow_single_execution_per_break_duration_in_halfopen_state__test_execution_permit_directly()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(1, durationOfBreak);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);

            // exception raised, circuit is now open.  
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // break duration passes, circuit now half open
            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);


            // OnActionPreExecute() should permit first execution.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // OnActionPreExecute() should reject a second execution.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // Allow another time window to pass (breaker should still be HalfOpen).
            SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // OnActionPreExecute() should now permit another trial execution.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
        }

        [Fact]
        public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__integration_test()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(1, durationOfBreak);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);

            // exception raised, circuit is now open.  
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // break duration passes, circuit now half open
            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // Start one execution during the HalfOpen state, and request a second execution before the first has completed (ie still during the HalfOpen state).
            // The second execution should be rejected due to the halfopen state.

            TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
            ManualResetEvent permitSecondExecutionAttempt = new ManualResetEvent(false);
            ManualResetEvent permitFirstExecutionEnd = new ManualResetEvent(false);

            bool? firstDelegateExecutedInHalfOpenState = null;
            bool? secondDelegateExecutedInHalfOpenState = null;
            bool? secondDelegateRejectedInHalfOpenState = null;

            bool firstExecutionActive = false;
            // First execution in HalfOpen state: we should be able to verify state is HalfOpen as it executes.
            Task firstExecution = Task.Factory.StartNew(() =>
            {
                breaker.Awaiting(x => x.ExecuteAsync(async () =>
                {
                    firstDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                    // Signal the second execution can start, overlapping with this (the first) execution.
                    firstExecutionActive = true;
                    permitSecondExecutionAttempt.Set();

                    // Hold first execution open until second indicates it is no longer needed, or time out.
                    permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks).Should().BeTrue();
                    await TaskHelper.EmptyTask;
                    firstExecutionActive = false;

                    return ResultPrimitive.Good;
                })).ShouldNotThrow();
            }, TaskCreationOptions.LongRunning);

            // Attempt a second execution, signalled by the first execution to ensure they overlap: we should be able to verify it doesn't execute, and is rejected by a breaker in a HalfOpen state.
            permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks).Should().BeTrue();

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
            Task.WaitAll(new[] { firstExecution, secondExecution }, testTimeoutToExposeDeadlocks).Should().BeTrue();
            if (firstExecution.IsFaulted) throw firstExecution.Exception;
            if (secondExecution.IsFaulted) throw secondExecution.Exception;
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
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(1, durationOfBreak);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);

            // exception raised, circuit is now open.  
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // break duration passes, circuit now half open
            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // Start one execution during the HalfOpen state.
            // Request a second execution while the first is still in flight (not completed), while still during the HalfOpen state, but after one breakDuration later.
            // The second execution should be accepted in the halfopen state due to being requested after one breakDuration later.

            TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
            ManualResetEvent permitSecondExecutionAttempt = new ManualResetEvent(false);
            ManualResetEvent permitFirstExecutionEnd = new ManualResetEvent(false);

            bool? firstDelegateExecutedInHalfOpenState = null;
            bool? secondDelegateExecutedInHalfOpenState = null;
            bool? secondDelegateRejectedInHalfOpenState = null;

            bool firstExecutionActive = false;
            // First execution in HalfOpen state: we should be able to verify state is HalfOpen as it executes.
            Task firstExecution = Task.Factory.StartNew(() =>
            {
                breaker.Awaiting(x => x.ExecuteAsync(async () =>
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
                })).ShouldNotThrow();
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
            Task.WaitAll(new[] { firstExecution, secondExecution }, testTimeoutToExposeDeadlocks).Should().BeTrue();
            if (firstExecution.IsFaulted) throw firstExecution.Exception;
            if (secondExecution.IsFaulted) throw secondExecution.Exception;
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
        public void Should_open_circuit_and_block_calls_if_manual_override_open()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // manually break circuit
            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            // circuit manually broken: execution should be blocked; even non-fault-returning executions should not reset circuit
            bool delegateExecutedWhenBroken = false;
            breaker.Awaiting(async b => await b.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return Task.FromResult(ResultPrimitive.Good); }).ConfigureAwait(false))
                .ShouldThrow<IsolatedCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.LastException.Should().BeOfType<IsolatedCircuitException>();
            delegateExecutedWhenBroken.Should().BeFalse();

        }

        [Fact]
        public void Should_hold_circuit_open_despite_elapsed_time_if_manual_override_open()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            bool delegateExecutedWhenBroken = false;
            breaker.Awaiting(async x => await x.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return Task.FromResult(ResultPrimitive.Good); }))
                .ShouldThrow<IsolatedCircuitException>();
            delegateExecutedWhenBroken.Should().BeFalse();
        }

        [Fact]
        public void Should_close_circuit_again_on_reset_after_manual_override()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                .ShouldThrow<IsolatedCircuitException>();

            breaker.Reset();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good))).ShouldNotThrow();
        }

        [Fact]
        public async Task Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception or fault raised, circuit is now open
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // reset circuit, with no time having passed
            breaker.Reset();
            SystemClock.UtcNow().Should().Be(time);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good))).ShouldNotThrow();
        }

        #endregion

        #region State-change delegate tests

        [Fact]
        public void Should_not_call_onreset_on_initialise()
        {
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            onResetCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Should_call_onbreak_when_breaking_circuit_automatically()
        {
            bool onBreakCalled = false;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().BeFalse();

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().BeTrue();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_manually()
        {
            bool onBreakCalled = false;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);
            onBreakCalled.Should().BeFalse();

            breaker.Isolate();

            onBreakCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_placed_through_open_circuit()
        {
            int onBreakCalled = 0;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            // call through circuit when already broken - should not retrigger onBreak 
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                  .ShouldThrow<BrokenCircuitException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);
        }

        [Fact]
        public async Task Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_call_failure_which_arrives_on_open_state_though_started_on_closed_state()
        {
            int onBreakCalled = 0;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1), onBreak, onReset);

            // Start an execution when the breaker is in the closed state, but hold it from returning (its failure) until the breaker has opened.  This call, a failure hitting an already open breaker, should indicate its fail, but should not cause onBreak() to be called a second time.
            TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
            ManualResetEvent permitLongRunningExecutionToReturnItsFailure = new ManualResetEvent(false);
            ManualResetEvent permitMainThreadToOpenCircuit = new ManualResetEvent(false);

            Task longRunningExecution = Task.Factory.StartNew(async () =>
            {
                breaker.CircuitState.Should().Be(CircuitState.Closed);

                (await breaker.ExecuteAsync(async () =>
                {
                    await TaskHelper.EmptyTask;

                    permitMainThreadToOpenCircuit.Set();

                    // Hold this execution until rest of the test indicates it can proceed (or timeout, to expose deadlocks).
                    permitLongRunningExecutionToReturnItsFailure.WaitOne(testTimeoutToExposeDeadlocks);

                    // Throw a further failure when rest of test has already broken the circuit.
                    breaker.CircuitState.Should().Be(CircuitState.Open);
                    return ResultPrimitive.Fault;

                })).Should().Be(ResultPrimitive.Fault); // However, since execution started when circuit was closed, BrokenCircuitException will not have been thrown on entry; the original fault should still be returned.
            }, TaskCreationOptions.LongRunning);

            permitMainThreadToOpenCircuit.WaitOne(testTimeoutToExposeDeadlocks).Should().BeTrue();

            // Break circuit in the normal manner: onBreak() should be called once.
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);
            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            // Permit the second (long-running) execution to hit the open circuit with its failure.
            permitLongRunningExecutionToReturnItsFailure.Set();

            // Graceful cleanup: allow executions time to end naturally; timeout if any deadlocks; expose any execution faults.  This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
            longRunningExecution.Wait(testTimeoutToExposeDeadlocks).Should().BeTrue();
            if (longRunningExecution.IsFaulted) throw longRunningExecution.Exception;
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
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().Be(0);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(0);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(1);

            // 2 exception or fault raised, circuit is now open
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset
            onResetCalled.Should().Be(0);

            // first call after duration is successful, so circuit should reset
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good))).ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().Be(1);
        }

        [Fact]
        public void Should_not_call_onreset_on_successive_successful_calls()
        {
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            onResetCalled.Should().BeFalse();

            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good))).ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();

            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good))).ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_subsequent_execution()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            int onHalfOpenCalled = 0;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

            onBreakCalled.Should().Be(0);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(0);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(1);

            // 2 exception or fault raised, circuit is now open
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            // duration has passed, circuit now half open
            onHalfOpenCalled.Should().Be(0); // not yet transitioned to half-open, because we have not queried state

            // first call after duration is successful, so circuit should reset
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                .ShouldNotThrow();
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
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

            onBreakCalled.Should().Be(0);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(0);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(1);

            // 2 exception or fault raised, circuit is now open
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            onHalfOpenCalled.Should().Be(1);
        }

        [Fact]
        public void Should_call_onreset_when_manually_resetting_circuit()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().Be(0);
            breaker.Isolate();
            onBreakCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                .ShouldThrow<IsolatedCircuitException>();

            onResetCalled.Should().Be(0);
            breaker.Reset();
            onResetCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good)))
                .ShouldNotThrow();
        }

        #region Tests of supplied parameters to onBreak delegate

        [Fact]
        public async Task Should_call_onbreak_with_the_last_handled_result()
        {
            ResultPrimitive? handledResult = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (outcome, _, __) => { handledResult = outcome.Result; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            handledResult?.Should().Be(ResultPrimitive.Fault);
        }

        [Fact]
        public async Task Should_call_onbreak_with_the_correct_timespan()
        {
            TimeSpan? passedBreakTimespan = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, timespan, __) => { passedBreakTimespan = timespan; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            passedBreakTimespan.Should().Be(durationOfBreak);
        }

        [Fact]
        public void Should_open_circuit_with_timespan_maxvalue_if_manual_override_open()
        {
            TimeSpan? passedBreakTimespan = null;
            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, timespan, __) => { passedBreakTimespan = timespan; };
            Action<Context> onReset = _ => { };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);
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
            IDictionary<string, object> contextData = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            (await breaker.RaiseResultSequenceAsync(new { key1 = "value1", key2 = "value2" }.AsDictionary(),
                ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public async Task Should_call_onreset_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, __, ___) => { };
            Action<Context> onReset = context => { contextData = context; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);
            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration should invoke onReset, with context
            await breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good), new { key1 = "value1", key2 = "value2" }.AsDictionary()).ConfigureAwait(false);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            IDictionary<string, object> contextData = new { key1 = "value1", key2 = "value2" }.AsDictionary();

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should().BeEmpty();
        }

        [Fact]
        public async Task Should_create_new_context_for_each_call_to_execute()
        {
            string contextValue = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, __, context) => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };
            Action<Context> onReset = context => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            // 2 exception or fault raised, circuit is now open
            (await breaker.RaiseResultSequenceAsync(new { key = "original_value" }.AsDictionary(), ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);
            contextValue.Should().Be("original_value");

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset

            // first call after duration is successful, so circuit should reset
            await breaker.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good), new { key = "new_value" }.AsDictionary()).ConfigureAwait(false);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            contextValue.Should().Be("new_value");
        }

        #endregion

        #endregion

        #region LastHandledResult property

        [Fact]
        public void Should_initialise_LastHandledResult_and_LastResult_to_default_on_creation()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.LastHandledResult.Should().Be(default(ResultPrimitive));
            breaker.LastException.Should().BeNull();
        }

        [Fact]
        public async Task Should_set_LastHandledResult_on_handling_result_even_when_not_breaking()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);


            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.LastHandledResult.Should().Be(ResultPrimitive.Fault);
            breaker.LastException.Should().BeNull();
        }

        [Fact]
        public async Task Should_set_LastHandledResult_to_last_handled_result_when_breaking()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.LastHandledResult.Should().Be(ResultPrimitive.Fault);
            breaker.LastException.Should().BeNull();
        }

        [Fact]
        public async Task Should_set_LastHandledResult_to_default_on_circuit_reset()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.LastHandledResult.Should().Be(ResultPrimitive.Fault);
            breaker.LastException.Should().BeNull();

            breaker.Reset();

            breaker.LastHandledResult.Should().Be(default(ResultPrimitive));
            breaker.LastException.Should().BeNull();
        }

        #endregion

        #region Cancellation support

        [Fact]
        public async Task Should_execute_action_when_non_faulting_and_cancellationtoken_not_cancelled()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = null,
            };

            (await breaker.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, 
                ResultPrimitive.Good).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Good);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationtoken_cancelled_before_execute()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
            };

            cancellationTokenSource.Cancel();

            breaker.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Good))
            .ShouldThrow<TaskCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationtoken()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            breaker.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Good,
                   ResultPrimitive.Good))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_action_execution_when_user_delegate_observes_cancellationtoken()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            breaker.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public async Task Should_report_faulting_from_faulting_action_execution_when_user_delegate_does_not_observe_cancellation()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            (await breaker.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault).ConfigureAwait(false))
                            .Should().Be(ResultPrimitive.Fault);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public async Task Should_report_cancellation_when_both_open_circuit_and_cancellation()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1));

            (await breaker.RaiseResultSequenceAsync(ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);

            breaker.Awaiting(async x => await x.RaiseResultSequenceAsync(ResultPrimitive.Fault))
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.");
            // Circuit is now broken.

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            cancellationTokenSource.Cancel();

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = null, // Cancelled manually instead - see above.
                ActionObservesCancellation = false
            };

            breaker.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_honour_different_cancellationtoken_captured_implicitly_by_action()
        {
            // Before CancellationToken support was built in to Polly, users of the library may have implicitly captured a CancellationToken and used it to cancel actions.  For backwards compatibility, Polly should not confuse these with its own CancellationToken; it should distinguish TaskCanceledExceptions thrown with different CancellationTokens.

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource policyCancellationTokenSource = new CancellationTokenSource();
            CancellationToken policyCancellationToken = policyCancellationTokenSource.Token;

            CancellationTokenSource implicitlyCapturedActionCancellationTokenSource = new CancellationTokenSource();
            CancellationToken implicitlyCapturedActionCancellationToken = implicitlyCapturedActionCancellationTokenSource.Token;

            implicitlyCapturedActionCancellationTokenSource.Cancel();

            int attemptsInvoked = 0;

            breaker.Awaiting(async x => await x.ExecuteAsync(async ct =>
            {
                attemptsInvoked++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                implicitlyCapturedActionCancellationToken.ThrowIfCancellationRequested();
                return ResultPrimitive.Good;
            }, policyCancellationToken))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(implicitlyCapturedActionCancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        #endregion
        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}