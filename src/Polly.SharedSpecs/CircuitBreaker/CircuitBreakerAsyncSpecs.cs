using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.CircuitBreaker
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class CircuitBreakerAsyncSpecs : IDisposable
    {
        #region Configuration tests

        [Fact]
        public void Should_be_able_to_handle_a_duration_of_timespan_maxvalue()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(1, TimeSpan.MaxValue);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_if_exceptions_allowed_before_breaking_is_less_than_one()
        {
           Action action = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .CircuitBreakerAsync(0, TimeSpan.FromSeconds(10));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                  .And.ParamName.Should()
                  .Be("exceptionsAllowedBeforeBreaking");
        }

        [Fact]
        public void Should_throw_if_duration_of_break_is_less_than_zero()
        {
            Action action = () => Policy
                                     .Handle<DivideByZeroException>()
                                     .CircuitBreakerAsync(1, -TimeSpan.FromSeconds(1));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("durationOfBreak");
        }

        [Fact]
        public void Should_be_able_to_handle_a_duration_of_break_of_zero()
        {
            Action action = () => Policy
                                     .Handle<DivideByZeroException>()
                                     .CircuitBreakerAsync(1, TimeSpan.Zero);
            action.ShouldNotThrow();
        }

        [Fact]
        public void Should_initialise_to_closed_state()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, durationOfBreak);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Circuit-breaker threshold-to-break tests

        [Fact]
        public void Should_not_open_circuit_if_specified_number_of_specified_exception_are_not_raised_consecutively()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async b => await b.ExecuteAsync(() => TaskHelper.EmptyTask)).ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_after_specified_number_of_specified_exception_have_been_raised()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            bool delegateExecutedWhenBroken = false;
            breaker.Awaiting(async x => await x.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }))
                  .ShouldThrow<BrokenCircuitException>()
                  .WithMessage("The circuit is now open and is not allowing calls.")
                  .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            delegateExecutedWhenBroken.Should().BeFalse();

        }

        [Fact]
        public void Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_after_specified_number_of_one_of_the_specified_exceptions_have_been_raised()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .Or<ArgumentOutOfRangeException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentOutOfRangeException>())
                  .ShouldThrow<ArgumentOutOfRangeException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            bool delegateExecutedWhenBroken = false;
            breaker.Awaiting(async x => await x.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }))
                  .ShouldThrow<BrokenCircuitException>()
                  .WithMessage("The circuit is now open and is not allowing calls.")
                  .WithInnerException<ArgumentOutOfRangeException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            delegateExecutedWhenBroken.Should().BeFalse();
        }

        [Fact]
        public void Should_not_open_circuit_if_exception_raised_is_not_the_specified_exception()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_not_open_circuit_if_exception_raised_is_not_one_of_the_specified_exceptions()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .Or<ArgumentOutOfRangeException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Circuit-breaker open->half-open->open/closed tests

        [Fact]
        public void Should_halfopen_circuit_after_the_specified_duration_has_passed()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // first call after duration raises an exception, so circuit should break again
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();

        }

        [Fact]
        public async Task Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_raise_an_exception()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // first call after duration is successful, so circuit should reset
            await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // circuit has been reset so should once again allow 2 exceptions to be raised before breaking
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_only_allow_single_execution_on_first_entering_halfopen_state__test_execution_permit_directly()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(1, durationOfBreak);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

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
        public void Should_allow_single_execution_per_break_duration_in_halfopen_state__test_execution_permit_directly()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(1, durationOfBreak);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

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
        public void Should_only_allow_single_execution_on_first_entering_halfopen_state__integration_test()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(1, durationOfBreak);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

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
                    permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                    await TaskHelper.EmptyTask;
                    firstExecutionActive = false;

                })).ShouldNotThrow();
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
        public void Should_allow_single_execution_per_break_duration_in_halfopen_state__integration_test()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(1, durationOfBreak);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

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

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // manually break circuit
            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            // circuit manually broken: execution should be blocked; even non-exception-throwing executions should not reset circuit
            bool delegateExecutedWhenBroken = false;
            breaker.Awaiting(async x => await x.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }))
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

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            bool delegateExecutedWhenBroken = false;
            breaker.Awaiting(async x => await x.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }))
                .ShouldThrow<IsolatedCircuitException>();
            delegateExecutedWhenBroken.Should().BeFalse();
        }

        [Fact]
        public void Should_close_circuit_again_on_reset_after_manual_override()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => TaskHelper.EmptyTask))
                .ShouldThrow<IsolatedCircuitException>();

            breaker.Reset();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => TaskHelper.EmptyTask)).ShouldNotThrow();
        }

        [Fact]
        public void Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // reset circuit, with no time having passed
            breaker.Reset();
            SystemClock.UtcNow().Should().Be(time);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => TaskHelper.EmptyTask)).ShouldNotThrow();
        }

        #endregion

        #region State-change delegate tests

        [Fact]
        public void Should_not_call_onreset_on_initialise()
        {
            Action<Exception, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            onResetCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_automatically()
        {
            bool onBreakCalled = false;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().BeFalse();

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().BeTrue();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_manually()
        {
            bool onBreakCalled = false;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);
            onBreakCalled.Should().BeFalse();

            breaker.Isolate();

            onBreakCalled.Should().BeTrue();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_placed_through_open_circuit()
        {
            int onBreakCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            // call through circuit when already broken - should not retrigger onBreak 
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_call_failure_which_arrives_on_open_state_though_started_on_closed_state()
        {
            int onBreakCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1), onBreak, onReset);

            // Start an execution when the breaker is in the closed state, but hold it from returning (its failure) until the breaker has opened.  This call, a failure hitting an already open breaker, should indicate its fail, but should not cause onBreak() to be called a second time.
            TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
            ManualResetEvent permitLongRunningExecutionToReturnItsFailure = new ManualResetEvent(false);
            ManualResetEvent permitMainThreadToOpenCircuit = new ManualResetEvent(false);

            Task longRunningExecution = Task.Factory.StartNew(() =>
            {
                breaker.CircuitState.Should().Be(CircuitState.Closed);

                breaker.Awaiting(x => x.ExecuteAsync(async () =>
                {
                    await TaskHelper.EmptyTask;

                    permitMainThreadToOpenCircuit.Set();

                    // Hold this execution until rest of the test indicates it can proceed (or timeout, to expose deadlocks).
                    permitLongRunningExecutionToReturnItsFailure.WaitOne(testTimeoutToExposeDeadlocks);

                    // Throw a further failure when rest of test has already broken the circuit.
                    breaker.CircuitState.Should().Be(CircuitState.Open);
                    throw new DivideByZeroException();

                })).ShouldThrow<DivideByZeroException>(); // However, since execution started when circuit was closed, BrokenCircuitException will not have been thrown on entry; the original exception will still be thrown.
            }, TaskCreationOptions.LongRunning);

            permitMainThreadToOpenCircuit.WaitOne(testTimeoutToExposeDeadlocks).Should().BeTrue();

            // Break circuit in the normal manner: onBreak() should be called once.
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
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
        public void Should_call_onreset_when_automatically_closing_circuit_but_not_when_halfopen()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().Be(0);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(0);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset
            onResetCalled.Should().Be(0);

            // first call after duration is successful, so circuit should reset
            breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().Be(1);
        }

        [Fact]
        public void Should_not_call_onreset_on_successive_successful_calls()
        {
            Action<Exception, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            onResetCalled.Should().BeFalse();

            breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();

            breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_subsequent_execution()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            int onHalfOpenCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

            onBreakCalled.Should().Be(0);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(0);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            // duration has passed, circuit now half open
            onHalfOpenCalled.Should().Be(0); // not yet transitioned to half-open, because we have not queried state

            // first call after duration is successful, so circuit should reset
            breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
            onHalfOpenCalled.Should().Be(1);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().Be(1);
        }

        [Fact]
        public void Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_state_read()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            int onHalfOpenCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

            onBreakCalled.Should().Be(0);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(0);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
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
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().Be(0);
            breaker.Isolate();
            onBreakCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => TaskHelper.EmptyTask))
                .ShouldThrow<IsolatedCircuitException>();

            onResetCalled.Should().Be(0);
            breaker.Reset();
            onResetCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Awaiting(async x => await x.ExecuteAsync(() => TaskHelper.EmptyTask)).ShouldNotThrow();
        }

        #region Tests of supplied parameters to onBreak delegate

        [Fact]
        public void Should_call_onbreak_with_the_last_raised_exception()
        {
            Exception passedException = null;

            Action<Exception, TimeSpan, Context> onBreak = (exception, _, __) => { passedException = exception; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            passedException?.Should().BeOfType<DivideByZeroException>();
        }

        [Fact]
        public void Should_rethrow_and_call_onbreak_with_the_last_raised_exception_unwrapped_if_matched_as_inner()
        {
            Exception passedException = null;

            Action<Exception, TimeSpan, Context> onBreak = (exception, _, __) => { passedException = exception; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .HandleInner<DivideByZeroException>()
                .Or<DivideByZeroException>()
                .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            Exception toRaiseAsInner = new DivideByZeroException();
            Exception withInner = new AggregateException(toRaiseAsInner);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();

            breaker.Awaiting(x => x.RaiseExceptionAsync(withInner))
                .ShouldThrow<DivideByZeroException>().Which.Should().BeSameAs(toRaiseAsInner);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            passedException?.Should().BeSameAs(toRaiseAsInner);
        }

        [Fact]
        public void Should_call_onbreak_with_the_correct_timespan()
        {
            TimeSpan? passedBreakTimespan = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, timespan, __) => { passedBreakTimespan = timespan; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            passedBreakTimespan.Should().Be(durationOfBreak);
        }

        [Fact]
        public void Should_open_circuit_with_timespan_maxvalue_if_manual_override_open()
        {
            TimeSpan? passedBreakTimespan = null;
            Action<Exception, TimeSpan, Context> onBreak = (_, timespan, __) => { passedBreakTimespan = timespan; };
            Action<Context> onReset = _ => { };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
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
        public void Should_call_onbreak_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>(
                new { key1 = "value1", key2 = "value2" }.AsDictionary()
                )).ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public async Task Should_call_onreset_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, ___) => { };
            Action<Context> onReset = context => { contextData = context; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration should invoke onReset, with context
            await breaker.ExecuteAsync(() => TaskHelper.EmptyTask, new { key1 = "value1", key2 = "value2" }.AsDictionary());

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            IDictionary<string, object> contextData = new { key1 = "value1", key2 = "value2" }.AsDictionary();

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should().BeEmpty();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_execute()
        {
            string contextValue = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };
            Action<Context> onReset = context => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            // 2 exception raised, circuit is now open
            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>(new { key = "original_value" }.AsDictionary()))
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            contextValue.Should().Be("original_value");

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset

            // first call after duration is successful, so circuit should reset
            breaker.ExecuteAsync(() => TaskHelper.EmptyTask, new { key = "new_value" }.AsDictionary());
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
        public void Should_set_LastException_on_handling_exception_even_when_not_breaking()
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.LastException.Should().BeOfType<DivideByZeroException>();
        }

        [Fact]
        public void Should_set_LastException_on_handling_inner_exception_even_when_not_breaking()
        {
            CircuitBreakerPolicy breaker = Policy
                .HandleInner<DivideByZeroException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            Exception toRaiseAsInner = new DivideByZeroException();
            Exception withInner = new AggregateException(toRaiseAsInner);

            breaker.Awaiting(x => x.RaiseExceptionAsync(withInner))
                .ShouldThrow<DivideByZeroException>().Which.Should().BeSameAs(toRaiseAsInner);

            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.LastException.Should().BeSameAs(toRaiseAsInner);
        }

        [Fact]
        public void Should_set_LastException_to_last_raised_exception_when_breaking()
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.LastException.Should().BeOfType<DivideByZeroException>();
        }

        [Fact]
        public void Should_set_LastException_to_null_on_circuit_reset()
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.LastException.Should().BeOfType<DivideByZeroException>();

            breaker.Reset();

            breaker.LastException.Should().BeNull();
        }

        #endregion

        #region Cancellation support

        [Fact]
        public void Should_execute_action_when_non_faulting_and_cancellationtoken_not_cancelled()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            breaker.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationtoken_cancelled_before_execute()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
            };

            cancellationTokenSource.Cancel();

            breaker.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationtoken()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            breaker.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_action_execution_when_user_delegate_observes_cancellationtoken()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            breaker.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_faulting_from_faulting_action_execution_when_user_delegate_does_not_observe_cancellation()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            breaker.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<DivideByZeroException>();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_when_both_open_circuit_and_cancellation()
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1));

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();

            breaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            // Circuit is now broken.

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            cancellationTokenSource.Cancel();

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null, // Cancelled manually instead - see above.
                ActionObservesCancellation = false
            };

            breaker.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_honour_different_cancellationtoken_captured_implicitly_by_action()
        {
            // Before CancellationToken support was built in to Polly, users of the library may have implicitly captured a CancellationToken and used it to cancel actions.  For backwards compatibility, Polly should not confuse these with its own CancellationToken; it should distinguish TaskCanceledExceptions thrown with different CancellationTokens.

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
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
                await TaskHelper.EmptyTask;
                implicitlyCapturedActionCancellationToken.ThrowIfCancellationRequested();
            }, policyCancellationToken))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(implicitlyCapturedActionCancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_execute_func_returning_value_when_cancellationtoken_not_cancelled()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            bool? result = null;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            breaker.Awaiting(async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true).ConfigureAwait(false))
                .ShouldNotThrow();

            result.Should().BeTrue();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_honour_and_report_cancellation_during_func_execution()
        {
            CircuitBreakerPolicy breaker = Policy
                             .Handle<DivideByZeroException>()
                             .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            bool? result = null;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            breaker.Awaiting(async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true).ConfigureAwait(false))
                .ShouldThrow<TaskCanceledException>().And.CancellationToken.Should().Be(cancellationToken);

            result.Should().Be(null);

            attemptsInvoked.Should().Be(1);
        }

        #endregion

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}